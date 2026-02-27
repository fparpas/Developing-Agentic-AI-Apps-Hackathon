"""
Secure Weather MCP Server - Challenge 09 Solution

Takes the Challenge 04 remote weather server and adds API key
authentication via pure ASGI middleware.
"""

import os
import hmac
from typing import Any
from contextlib import asynccontextmanager

import httpx
import uvicorn
from fastapi import FastAPI
from fastapi.responses import JSONResponse
from mcp.server.fastmcp import FastMCP
from dotenv import load_dotenv

load_dotenv()

API_KEY = os.getenv("API_KEY", "SuperSecureSecretUsedAsApiKey1")

mcp = FastMCP("weather")

NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-app/1.0"


async def make_nws_request(url: str) -> dict[str, Any] | None:
    headers = {"User-Agent": USER_AGENT, "Accept": "application/geo+json"}
    async with httpx.AsyncClient() as client:
        try:
            response = await client.get(url, headers=headers, timeout=30.0)
            response.raise_for_status()
            return response.json()
        except Exception:
            return None


def format_alert(feature: dict) -> str:
    props = feature["properties"]
    return (
        f"Event: {props.get('event', 'Unknown')}\n"
        f"Area: {props.get('areaDesc', 'Unknown')}\n"
        f"Severity: {props.get('severity', 'Unknown')}\n"
        f"Description: {props.get('description', 'N/A')}\n"
        f"Instructions: {props.get('instruction', 'N/A')}"
    )


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
    return "\n---\n".join(format_alert(f) for f in data["features"])


@mcp.tool()
async def get_forecast(latitude: float, longitude: float) -> str:
    """Get weather forecast for a location.

    Args:
        latitude: Latitude of the location
        longitude: Longitude of the location
    """
    points_data = await make_nws_request(f"{NWS_API_BASE}/points/{latitude},{longitude}")
    if not points_data:
        return "Unable to fetch forecast data for this location."
    try:
        forecast_url = points_data["properties"]["forecast"]
    except KeyError:
        return "Unable to determine forecast URL for this location."

    forecast_data = await make_nws_request(forecast_url)
    if not forecast_data:
        return "Unable to fetch detailed forecast."

    forecasts = []
    for p in forecast_data["properties"]["periods"][:5]:
        forecasts.append(
            f"{p['name']}:\n"
            f"  Temperature: {p['temperature']}°{p['temperatureUnit']}\n"
            f"  Wind: {p['windSpeed']} {p['windDirection']}\n"
            f"  Forecast: {p['detailedForecast']}"
        )
    return "\n---\n".join(forecasts)


mcp_app = mcp.streamable_http_app()

@asynccontextmanager
async def lifespan(app):
    async with mcp_app.router.lifespan_context(mcp_app):
        yield

app = FastAPI(title="Weather MCP Server", version="1.0.0", lifespan=lifespan)

@app.get("/health")
async def health_check():
    return {"status": "healthy"}


class ApiKeyAuthMiddleware:
    """
    ASGI middleware that validates an X-API-Key header on protected paths.
    """

    def __init__(self, app, api_key: str, protected_paths: list[str] = None):
        self.app = app
        self.api_key = api_key
        self.protected_paths = protected_paths or ["/mcp"]

    async def __call__(self, scope, receive, send):
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return

        if not any(scope["path"].startswith(p) for p in self.protected_paths):
            await self.app(scope, receive, send)
            return

        headers = dict(scope["headers"])
        key = headers.get(b"x-api-key")

        if not key or not hmac.compare_digest(key.decode(), self.api_key):
            resp = JSONResponse(
                status_code=401,
                content={"detail": "Missing or invalid API key. Provide 'X-API-Key' header."},
                headers={"WWW-Authenticate": "ApiKey realm=\"API\""},
            )
            await resp(scope, receive, send)
            return

        await self.app(scope, receive, send)


# Mount MCP app and wrap everything with auth middleware
app.mount("/", mcp_app)
app = ApiKeyAuthMiddleware(app, api_key=API_KEY, protected_paths=["/mcp"])


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")
