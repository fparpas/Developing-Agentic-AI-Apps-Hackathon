"""
Secure Weather MCP Server - Challenge 09 Solution

This server demonstrates how to implement API key authentication
for a remote MCP server using FastAPI and custom middleware.

Key features:
- API key validation via X-API-Key header
- FastAPI with FastMCP integration
- National Weather Service API integration
- Secure endpoint protection with middleware
"""

import os
from typing import Any
import httpx
from fastapi.responses import JSONResponse
from mcp.server.fastmcp import FastMCP
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# ============================================================================
# Configuration
# ============================================================================

# API Key for authentication (load from environment for security)
API_KEY = os.getenv("API_KEY", "SuperSecureSecretUsedAsApiKey1")

# National Weather Service API configuration
NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-mcp-server/1.0"

# ============================================================================
# API Key Authentication Middleware
# ============================================================================

class ApiKeyAuthMiddleware:
    """
    ASGI Middleware to validate API key from request headers.

    Implemented as pure ASGI middleware to support streaming responses (SSE)
    which can be problematic with BaseHTTPMiddleware.
    """

    def __init__(self, app, api_key: str, protected_paths: list[str] = None):
        self.app = app
        self.api_key = api_key
        self.protected_paths = protected_paths or ["/mcp"]

    async def __call__(self, scope, receive, send):
        # Only handle HTTP requests
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return

        path = scope["path"]
        # Check if path requires authentication
        requires_auth = any(path.startswith(p) for p in self.protected_paths)

        if not requires_auth:
            await self.app(scope, receive, send)
            return

        # Validate API Key
        headers = dict(scope["headers"])
        # Headers are bytes and lower-case in ASGI
        key_header = headers.get(b"x-api-key")

        if not key_header or key_header.decode() != self.api_key:
            response = JSONResponse(
                status_code=401,
                content={"detail": "Invalid or missing API Key, expected in X-API-Key header."},
                headers={"WWW-Authenticate": "ApiKey realm=\"API\""}
            )
            await response(scope, receive, send)
            return

        await self.app(scope, receive, send)

# ============================================================================
# Weather API Helper Functions
# ============================================================================

async def make_nws_request(url: str) -> dict[str, Any] | None:
    """
    Make a request to the National Weather Service API.

    Args:
        url: The full URL to request from NWS API

    Returns:
        The parsed JSON response, or None if request fails
    """
    headers = {
        "User-Agent": USER_AGENT,
        "Accept": "application/geo+json"
    }

    async with httpx.AsyncClient() as client:
        try:
            response = await client.get(url, headers=headers, timeout=30.0)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            print(f"WARNING: Failed to fetch from NWS API: {e}")
            return None

def format_alert(feature: dict) -> str:
    """
    Format a weather alert feature into a readable string.

    Args:
        feature: A GeoJSON feature object from NWS alerts API

    Returns:
        Formatted string containing alert information
    """
    props = feature["properties"]
    return f"""
Event: {props.get('event', 'Unknown')}
Area: {props.get('areaDesc', 'Unknown')}
Severity: {props.get('severity', 'Unknown')}
Description: {props.get('description', 'No description available')}
Instructions: {props.get('instruction', 'No specific instructions provided')}
"""

# ============================================================================
# Initialize FastMCP Server
# ============================================================================

# Create FastMCP instance for tool registration
mcp = FastMCP("weather")

# ============================================================================
# MCP Tools - Weather Functionality
# ============================================================================

@mcp.tool()
async def get_alerts(state: str) -> str:
    """
    Get weather alerts for a US state.

    This tool queries the National Weather Service API for active
    severe weather alerts affecting the specified state.

    Args:
        state: Two-letter US state code (e.g., CA, NY, WA, TX)

    Returns:
        Formatted string containing all active alerts for the state,
        or a message indicating no alerts are present
    """
    url = f"{NWS_API_BASE}/alerts/active/area/{state}"
    data = await make_nws_request(url)

    if not data or "features" not in data:
        return "Unable to fetch alerts or no alerts found."

    if not data["features"]:
        return "No active alerts for this state."

    # Format each alert and join them
    alerts = [format_alert(feature) for feature in data["features"]]
    return "\n---\n".join(alerts)


@mcp.tool()
async def get_forecast(latitude: float, longitude: float) -> str:
    """
    Get weather forecast for a location.

    This tool retrieves a detailed weather forecast using the
    National Weather Service API for the specified coordinates.

    Args:
        latitude: Latitude of the location (e.g., 47.6062 for Seattle)
        longitude: Longitude of the location (e.g., -122.3321 for Seattle)

    Returns:
        Formatted string containing the next 5 forecast periods,
        or an error message if the forecast cannot be retrieved
    """
    # Step 1: Get the forecast grid endpoint for the coordinates
    points_url = f"{NWS_API_BASE}/points/{latitude},{longitude}"
    points_data = await make_nws_request(points_url)

    if not points_data:
        return "Unable to fetch forecast data for this location."

    try:
        forecast_url = points_data["properties"]["forecast"]
    except KeyError:
        return "Unable to determine forecast URL for this location."

    # Step 2: Fetch the actual forecast data
    forecast_data = await make_nws_request(forecast_url)
    if not forecast_data:
        return "Unable to fetch detailed forecast."

    # Format forecast periods
    periods = forecast_data["properties"]["periods"]
    forecasts = []

    # Show the next 5 forecast periods
    for period in periods[:5]:
        forecast = f"""
{period['name']}:
Temperature: {period['temperature']}°{period['temperatureUnit']}
Wind: {period['windSpeed']} {period['windDirection']}
Forecast: {period['detailedForecast']}
"""
        forecasts.append(forecast)

    return "\n---\n".join(forecasts)

# ============================================================================
# FastAPI Application Setup
# ============================================================================

# Get the FastMCP Starlette app that exposes the /mcp endpoint
mcp_app = mcp.streamable_http_app()

@mcp_app.route("/")
async def root(request):
    """Root endpoint providing server information."""
    return JSONResponse(
        {
            "name": "Secure Weather MCP Server",
            "version": "1.0.0",
            "description": "MCP server providing weather forecasts and alerts",
            "authentication": "Required for MCP endpoints (X-API-Key header)",
            "tools": ["get_forecast", "get_alerts"],
            "status": "running"
        }
    )


@mcp_app.route("/health")
async def health_check(request):
    """Health check endpoint for monitoring."""
    return JSONResponse(
        {
            "status": "healthy",
            "service": "weather-mcp-server"
        }
    )

# Wrap the MCP app with API key middleware so /mcp requires authentication
app = ApiKeyAuthMiddleware(mcp_app, api_key=API_KEY, protected_paths=["/mcp"])

# ============================================================================
# Main Entry Point
# ============================================================================

if __name__ == "__main__":
    import uvicorn

    # Get port from environment or use default
    port = int(os.getenv("PORT", "5000"))

    print("=" * 60)
    print("Secure Weather MCP Server - Challenge 09 Solution")
    print("=" * 60)
    print(f"Starting server on port {port}")
    print(f"API Key authentication: ENABLED")
    print(f"Protected endpoints: /mcp/*")
    print(f"Public endpoints: /, /health")
    print("=" * 60)

    # Run the server
    # Use 0.0.0.0 to accept connections from any interface
    # This is necessary for remote access
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=port,
        log_level="info"
    )
