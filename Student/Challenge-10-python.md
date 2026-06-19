# Challenge 10 - Python - Secure your remote MCP server with an API Key

[< Previous Challenge](./Challenge-09-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-11-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-10-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-10-python.md)

## Introduction

Previously, you worked with MCP servers and clients in both local and remote environments, but these setups lacked authentication and authorization. While this approach might suffice for development or trusted local scenarios, it is not suitable for production. When deploying remote MCP servers accessible over HTTP/HTTPS, it is essential to implement robust authentication for all clients. Exposing an unsecured MCP server to the internet can result in significant security risks (abuse of tools, data exfiltration, quota exhaustion, malicious chaining, etc.).

In this challenge, you will secure your MCP Weather Server by introducing API key authentication. This gives you a simple, explicit credential mechanism while preparing the code structure so you can later upgrade to standards-based authorization (OAuth 2.1 / OIDC, signed tokens, per-principal policies) with minimal refactoring.

> ℹ️ API keys are intentionally used here as the “training wheels” step before adopting a full identity provider such as Microsoft Entra ID with OAuth 2.1 / OIDC. Keep your handler boundaries clean so you can drop in a standards-compliant validator later without touching business logic.

## Concepts

### API Key Authentication

API keys are a simple and effective method for authenticating clients to your MCP server. They provide:

- **Client Identification**: Each client gets a unique key to identify requests
- **Access Control**: Keys can be revoked or have different permission levels
- **Usage Tracking**: Monitor which clients are making requests
- **Rate Limiting**: Control request frequency per client

### MCP Security Considerations
When securing MCP servers, consider:
- **Transport Security**: Use HTTPS for encrypted communication
- **Authentication**: Verify client identity before processing requests
- **Authorization**: Control which tools/resources clients can access
- **Input Validation**: Sanitize all inputs to prevent injection attacks
- **Audit Logging**: Track all requests for security monitoring
- **Rate Limiting**: Prevent abuse and DoS attacks

> (IMPORTANT) Canonical header name: use `X-API-Key` everywhere. Both server and client must match the exact casing to avoid authentication failures—double-check configuration before deploying.

#### MCP Server Authorization (High Level)

The Model Context Protocol includes an authorization model aligned with OAuth concepts for HTTP transports. This challenge deliberately starts simpler (static API key) so you can focus on the mechanics of securing endpoints. Your handler, routing, and middleware ordering should make it trivial to swap in a standards-compliant token validator later. See the specs: [MCP Authorization Standards Compliance](https://modelcontextprotocol.io/specification/2025-06-18/basic/authorization#standards-compliance).

## Description
In this challenge, you will upgrade your existing Weather MCP Server to enable secure remote access using API key authentication.

You will upgrade your existing (previous challenge) Weather MCP Server to require an API key for every MCP request. The work includes:

1. Converting (or confirming) the server is exposed via HTTP (remote capable) and not only local process transport.
2. Adding middleware that validates an API key from a header.
3. Registering the authentication middleware in the correct order.
4. Requiring authorization for the MCP endpoint route.
5. Updating your MCP client to send the header.

> ℹ️ While API keys are a simple way to secure your server, it is generally more secure to authenticate clients using an identity provider such as Microsoft Entra ID (formerly Azure AD) with OAuth 2.0 or OpenID Connect flows. These modern authentication methods provide stronger security, support for user and application identities, token expiration, and advanced access controls. For production scenarios, consider integrating with an identity provider instead of relying solely on API keys.

## Tasks

### Task 1: Convert MCP Server to remote MCP Server

Ensure that your MCP server is converted into remote MCP server that can handle HTTP requests (this was accomplished in Challenge 04):

### Task 2: Add API Key Authentication Middleware to your Remote MCP Server

Take your `weather_remote_server.py` from Challenge 04 and add API key protection. You need three things on top of the existing code:

1. An ASGI middleware class that checks the `X-API-Key` header
2. Load the expected key from an environment variable
3. Wrap the app with the middleware before passing it to uvicorn

Below is the complete server.

**File: `secure_weather_server.py`**

```python
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

API_KEY = os.getenv("API_KEY", "your-secure-api-key-change-this")

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
    """Pure ASGI middleware that validates an X-API-Key header on protected paths.

    Uses raw ASGI (not BaseHTTPMiddleware) so streaming responses (SSE) work correctly.
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
                content={"detail": "Missing or invalid API key. Provide 'X-API-Key' header."}
            )
            await resp(scope, receive, send)
            return

        await self.app(scope, receive, send)


# Mount MCP app and wrap everything with auth middleware
app.mount("/", mcp_app)
app = ApiKeyAuthMiddleware(app, api_key=API_KEY, protected_paths=["/mcp"])


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")
```

### Task 3: Update the MCP Client to send the API Key

The only change to your existing client is passing the API key header when creating the MCP tool. Use `MCPStreamableHTTPTool` with an `httpx.AsyncClient` that carries the auth header:

```python
"""Secure MCP Client - connects to a remote MCP server with API key auth."""

import asyncio
import os
import httpx
from agent_framework import Agent, MCPStreamableHTTPTool
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

load_dotenv()


async def main():
    chat_client = AzureOpenAIResponsesClient(
        endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
        api_key=os.environ["AZURE_OPENAI_API_KEY"],
        deployment_name=os.environ["AZURE_OPENAI_DEPLOYMENT_NAME"],
        api_version="latest"
    )

    # The only change vs. an unsecured client: pass the API key via headers
    server_url = os.getenv("MCP_SERVER_URL", "http://localhost:8000/mcp")
    http_client = httpx.AsyncClient(headers={"x-api-key": os.environ["API_KEY"]})
    mcp_tool = MCPStreamableHTTPTool(
        name="WeatherMCP",
        url=server_url,
        http_client=http_client
        # Once agent-framework 1.0.0 is released, you should be able to
        # pass headers here directly, instead of an http_client, like this:
        # headers={"x-api-key": os.environ["API_KEY"]}
    )

    async with Agent(
        client=chat_client,
        name="WeatherAgent",
        instructions="You are a helpful weather assistant with access to weather tools.",
        tools=[mcp_tool]
    ) as agent:
        query = "What's the weather forecast for New York?"
        print(f"Query: {query}\n")
        async for update in agent.run(query, stream=True):
            if update.text:
                print(update.text, end="", flush=True)
        print()


if __name__ == "__main__":
    asyncio.run(main())
```

### Task 4: Verify secure communication between MCP Client and Server

To complete this challenge, make sure your MCP client is configured to include the API key in the request headers when communicating with the remote, secured MCP server. After updating your client, test the connection by sending requests to the server:

- If the API key is missing or incorrect, the server should respond with an authentication error (HTTP 401 Unauthorized).
- If the API key is valid, your client should receive successful responses from the protected MCP endpoints.

Verify that only requests with the correct API key are processed, confirming that your authentication mechanism is working as intended. This demonstrates secure communication between your MCP client and the remote server.

## Success Criteria
- ✅ Requests without API keys are rejected (authentication enforced)
- ✅ Only valid API keys can access protected endpoints (authorization verified)
- ✅ API key authentication system is implemented and functional
- ✅ All MCP endpoints require valid authentication
- ✅ MCP client successfully connects to the remote secured server

## Learning Resources

- [Starlette Middleware](https://www.starlette.io/middleware/)
- [OWASP API Security Best Practices](https://owasp.org/www-project-api-security/)
- [Azure Key Vault for Secrets Management](https://learn.microsoft.com/en-us/azure/key-vault/)
- [HTTP Security Headers](https://owasp.org/www-project-secure-headers/)
- [Model Context Protocol Security Guidelines](https://modelcontextprotocol.io/docs/security)
- [Python HMAC for Constant-Time Comparison](https://docs.python.org/3/library/hmac.html)
- [uvicorn Documentation](https://www.uvicorn.org/)
- [httpx HTTP Client](https://www.python-httpx.org/)
- [Async Python Security Patterns](https://python.readthedocs.io/en/stable/library/asyncio.html)