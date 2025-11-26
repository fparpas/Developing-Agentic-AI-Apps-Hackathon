"""
Weather MCP Server - Challenge 02 Solution

This module implements a Model Context Protocol (MCP) server that provides
weather forecasting and alert tools using the National Weather Service API.

The server exposes two tools:
- get_forecast: Returns detailed weather forecast for given coordinates
- get_alerts: Returns active severe weather alerts for a US state

The server communicates via stdio transport, allowing MCP hosts like Claude
Desktop or VS Code Copilot Chat to discover and invoke the tools.
"""

from typing import Any
import httpx
from mcp.server.fastmcp import FastMCP
import sys

# Initialize FastMCP server - automatically handles stdio transport
# The name "weather" identifies this server to MCP hosts
mcp = FastMCP("weather")

# Constants for the National Weather Service API
NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-tool/1.0"


async def make_nws_request(url: str) -> dict[str, Any] | None:
    """Make a request to the NWS API with proper error handling.

    This helper function handles HTTP requests to the National Weather Service
    API with appropriate headers, timeout, and error handling. It's used by
    both weather tools to fetch real-time data.

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
            # Return None on any error - the calling function will handle it
            return None


def format_alert(feature: dict) -> str:
    """Format an alert feature into a readable string.

    Takes a GeoJSON feature from the NWS API and extracts key
    alert information (event, area, severity, description, and
    instructions) for display to the user.

    Args:
        feature: A GeoJSON feature object from the NWS alerts API

    Returns:
        A formatted string containing the alert information
    """
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

    This tool queries the National Weather Service API for active severe weather
    alerts affecting the specified US state. It returns formatted information
    about each alert including the event type, affected area, severity level,
    and recommended instructions.

    Args:
        state: Two-letter US state code (e.g., CA, NY, WA, TX, OH)

    Returns:
        A formatted string containing all active alerts for the state,
        or a message indicating no alerts are present
    """
    # Construct URL to fetch alerts for the specified state
    url = f"{NWS_API_BASE}/alerts/active/area/{state}"
    data = await make_nws_request(url)

    # Handle API errors or missing data
    if not data or "features" not in data:
        return "Unable to fetch alerts or no alerts found."

    # Return appropriate message if no active alerts exist
    if not data["features"]:
        return "No active alerts for this state."

    # Format each alert and join them with a separator for readability
    alerts = [format_alert(feature) for feature in data["features"]]
    return "\n---\n".join(alerts)


@mcp.tool()
async def get_forecast(latitude: float, longitude: float) -> str:
    """Get weather forecast for a location.

    This tool retrieves a detailed weather forecast for a specific geographic
    location using the National Weather Service API. It returns the next 5
    forecast periods with temperature, wind conditions, and detailed forecast
    text.

    The NWS API works in two steps:
    1. Get the forecast grid endpoint for the given coordinates
    2. Fetch the detailed forecast from that endpoint

    Args:
        latitude: Latitude of the location (e.g., 47.6062 for Seattle)
        longitude: Longitude of the location (e.g., -122.3321 for Seattle)

    Returns:
        A formatted string containing the next 5 forecast periods,
        or an error message if the forecast cannot be retrieved
    """
    # Step 1: Get the forecast grid endpoint for the given coordinates
    # The NWS API requires us to first look up the forecast URL for a location
    points_url = f"{NWS_API_BASE}/points/{latitude},{longitude}"
    points_data = await make_nws_request(points_url)

    # Handle errors fetching point data
    if not points_data:
        return "Unable to fetch forecast data for this location."

    # Extract the forecast URL from the points response
    try:
        forecast_url = points_data["properties"]["forecast"]
    except KeyError:
        return "Unable to determine forecast URL for this location."

    # Step 2: Fetch the actual forecast data using the URL from step 1
    forecast_data = await make_nws_request(forecast_url)
    if not forecast_data:
        return "Unable to fetch detailed forecast."

    # Format the forecast periods into readable text
    periods = forecast_data["properties"]["periods"]
    forecasts = []

    # Show the next 5 forecast periods to avoid overwhelming the user
    for period in periods[:5]:
        forecast = f"""
{period['name']}:
Temperature: {period['temperature']}°{period['temperatureUnit']}
Wind: {period['windSpeed']} {period['windDirection']}
Forecast: {period['detailedForecast']}
"""
        forecasts.append(forecast)

    return "\n---\n".join(forecasts)


def main():
    """Initialize and run the MCP server.

    This function starts the server listening on stdio, which allows MCP hosts
    (like Claude Desktop or VS Code Copilot Chat) to communicate with it via
    JSON-RPC messages. The server will automatically discover and register
    the tools decorated with @mcp.tool().
    """
    print('[INFO] MCP server is up.', file=sys.stderr)
    mcp.run(transport='stdio')


if __name__ == "__main__":
    main()
