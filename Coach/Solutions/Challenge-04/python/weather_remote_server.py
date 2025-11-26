"""
Remote MCP Server for Azure Deployment - Challenge 04 Solution

This module implements an MCP server using FastAPI for HTTP transport,
making it suitable for deployment on Azure Container Apps or Azure Functions.

The server is containerized and runs as a web service, allowing remote
clients to access weather tools via HTTP instead of stdio.
"""

from typing import Any
import httpx
from contextlib import asynccontextmanager

from fastapi import FastAPI
from mcp.server.fastmcp import FastMCP

# Initialize FastMCP server with FastAPI
mcp = FastMCP("weather")

# Constants for the National Weather Service API
NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-app/1.0"


async def make_nws_request(url: str) -> dict[str, Any] | None:
    """Make a request to the NWS API with proper error handling.

    This helper function handles HTTP requests to the National Weather Service
    API with appropriate headers, timeout, and error handling.

    Args:
        url: The full URL to request from the NWS API

    Returns:
        The parsed JSON response, or None if the request fails
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
        except Exception:
            return None


def format_alert(feature: dict) -> str:
    """Format an alert feature into a readable string."""
    props = feature["properties"]
    return f"""
Event: {props.get('event', 'Unknown')}
Area: {props.get('areaDesc', 'Unknown')}
Severity: {props.get('severity', 'Unknown')}
Description: {props.get('description', 'No description available')}
Instructions: {props.get('instruction', 'No specific instructions provided')}
"""


@mcp.tool()
async def get_alerts(state: str) -> str:
    """Get weather alerts for a US state.

    Args:
        state: Two-letter US state code (e.g., CA, NY, WA, TX)
    """
    url = f"{NWS_API_BASE}/alerts/active/area/{state}"
    data = await make_nws_request(url)

    if not data or "features" not in data:
        return "Unable to fetch alerts or no alerts found."

    if not data["features"]:
        return "No active alerts for this state."

    alerts = [format_alert(feature) for feature in data["features"]]
    return "\n---\n".join(alerts)


@mcp.tool()
async def get_forecast(latitude: float, longitude: float) -> str:
    """Get weather forecast for a location.

    Args:
        latitude: Latitude of the location
        longitude: Longitude of the location
    """
    points_url = f"{NWS_API_BASE}/points/{latitude},{longitude}"
    points_data = await make_nws_request(points_url)

    if not points_data:
        return "Unable to fetch forecast data for this location."

    try:
        forecast_url = points_data["properties"]["forecast"]
    except KeyError:
        return "Unable to determine forecast URL for this location."

    forecast_data = await make_nws_request(forecast_url)
    if not forecast_data:
        return "Unable to fetch detailed forecast."

    periods = forecast_data["properties"]["periods"]
    forecasts = []

    for period in periods[:5]:
        forecast = f"""
{period['name']}:
Temperature: {period['temperature']}°{period['temperatureUnit']}
Wind: {period['windSpeed']} {period['windDirection']}
Forecast: {period['detailedForecast']}
"""
        forecasts.append(forecast)

    return "\n---\n".join(forecasts)


# Create FastAPI application
app = FastAPI(title="Weather MCP Server", version="1.0.0")


@app.get("/health")
async def health_check():
    """Health check endpoint for Azure deployment."""
    return {"status": "healthy", "service": "weather-mcp-server"}


@app.get("/")
async def root():
    """Root endpoint providing server information."""
    return {
        "name": "Weather MCP Server",
        "version": "1.0.0",
        "description": "MCP server providing weather forecasts and alerts",
        "tools": ["get_forecast", "get_alerts"],
        "documentation": "/docs"
    }


# Integrate MCP with FastAPI
@asynccontextmanager
async def lifespan(app: FastAPI):
    """Lifespan context manager for FastAPI."""
    yield


app.router.lifespan_context = lifespan


if __name__ == "__main__":
    import uvicorn

    # Run the server on port 8000 (Azure will expose it on port 80)
    # Use 0.0.0.0 to accept connections from any interface
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=8000,
        log_level="info"
    )
