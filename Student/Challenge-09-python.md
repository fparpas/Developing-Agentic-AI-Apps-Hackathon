# Challenge 09 - Python - Secure your remote MCP server with an API Key

[< Previous Challenge](./Challenge-08-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-10-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-09-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-09-python.md)

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

### Task 2: Implement API Key Authentication Middleware

Create authentication middleware to validate API keys from the `X-API-Key` header.

**File: `auth_middleware.py`**

```python
"""
API Key Authentication Middleware

This module provides authentication middleware for securing MCP server endpoints.
Implemented as pure ASGI middleware to support streaming responses (SSE).
"""

from starlette.responses import JSONResponse


class ApiKeyAuthMiddleware:
    """ASGI Middleware to validate API key from request headers.

    Implemented as pure ASGI middleware (not BaseHTTPMiddleware) to properly
    support streaming responses like Server-Sent Events used by MCP.

    For production scenarios, consider:
    1. Storing keys securely (Azure Key Vault, database with hashing, etc.).
    2. Supporting key rotation (multiple active keys with expirations).
    3. Adding rate limiting and anomaly detection per key.
    4. Moving to a stronger, token-based (OAuth 2.1 / OIDC) authorization model
       when user/app identity is required.
    """

    def __init__(self, app, api_key: str, protected_paths: list[str] = None):
        """Initialize the middleware.

        Args:
            app: The ASGI application
            api_key: The valid API key to check against
            protected_paths: List of path prefixes that require authentication.
                           Default is ["/mcp"]
        """
        self.app = app
        self.api_key = api_key
        self.protected_paths = protected_paths or ["/mcp"]

    async def __call__(self, scope, receive, send):
        """Process the request and validate API key if needed."""

        # Only handle HTTP requests
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return

        path = scope["path"]
        # Check if this path requires authentication
        requires_auth = any(path.startswith(p) for p in self.protected_paths)

        if not requires_auth:
            # Public endpoints don't require authentication
            await self.app(scope, receive, send)
            return

        # Extract API key from header
        # Headers are bytes and lower-case in ASGI
        headers = dict(scope["headers"])
        api_key_header = headers.get(b"x-api-key")

        if not api_key_header:
            response = JSONResponse(
                status_code=401,
                content={"detail": "Missing API key. Provide 'X-API-Key' header."},
                headers={"WWW-Authenticate": "ApiKey realm=\"API\""}
            )
            await response(scope, receive, send)
            return

        # Validate API key (use constant-time comparison)
        if not self._validate_key(api_key_header.decode()):
            response = JSONResponse(
                status_code=401,
                content={"detail": "Invalid API key"},
                headers={"WWW-Authenticate": "ApiKey realm=\"API\""}
            )
            await response(scope, receive, send)
            return

        # Key is valid, proceed to the application
        await self.app(scope, receive, send)

    def _validate_key(self, provided_key: str) -> bool:
        """Validate the provided API key.

        Replace this implementation with a secure lookup from a store
        (database, Key Vault, etc.). Use constant-time comparison to
        minimize timing attack vectors.
        """
        import hmac
        return hmac.compare_digest(provided_key, self.api_key)
```

### Task 3: Integrate Authentication Middleware and Protect MCP Endpoints

Integrate the authentication middleware into your remote MCP server.

**File: `secure_weather_server.py`**

```python
"""
Secure Remote MCP Weather Server - Challenge 09

This module implements a secure MCP server using FastMCP with API key authentication.
The server requires a valid X-API-Key header for all MCP requests.
"""

import os
from typing import Any
import httpx

from mcp.server.fastmcp import FastMCP
from starlette.responses import JSONResponse
from dotenv import load_dotenv

from auth_middleware import ApiKeyAuthMiddleware

load_dotenv()

# Configuration
API_KEY = os.getenv("API_KEY", "your-secure-api-key-change-this")
NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-mcp-server/1.0"

# Initialize FastMCP server
mcp = FastMCP("weather")


async def make_nws_request(url: str) -> dict[str, Any] | None:
    """Make a request to the NWS API with proper error handling."""
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

    return str(data["features"])


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


# Get the Starlette app from FastMCP (exposes /mcp endpoint)
mcp_app = mcp.streamable_http_app()


@mcp_app.route("/")
async def root(request):
    """Root endpoint providing server information (no authentication required)."""
    return JSONResponse({
        "name": "Secure Weather MCP Server",
        "version": "1.0.0",
        "description": "MCP server providing weather forecasts and alerts",
        "authentication": "Required for MCP endpoints (X-API-Key header)",
        "tools": ["get_forecast", "get_alerts"],
        "status": "running"
    })


@mcp_app.route("/health")
async def health_check(request):
    """Health check endpoint (no authentication required)."""
    return JSONResponse({
        "status": "healthy",
        "service": "weather-mcp-server"
    })


# Wrap the Starlette app with API key middleware
# This protects the /mcp endpoint while leaving / and /health public
app = ApiKeyAuthMiddleware(mcp_app, api_key=API_KEY, protected_paths=["/mcp"])


if __name__ == "__main__":
    import uvicorn

    port = int(os.getenv("PORT", "5000"))

    print("=" * 60)
    print("Secure Weather MCP Server - Challenge 09")
    print("=" * 60)
    print(f"Starting server on port {port}")
    print(f"API Key authentication: ENABLED")
    print(f"Protected endpoints: /mcp/*")
    print(f"Public endpoints: /, /health")
    print("=" * 60)

    uvicorn.run(
        app,
        host="0.0.0.0",
        port=port,
        log_level="info"
    )
```

### Task 4: Update the MCP Client to send the API Key

Modify your MCP client created in a previous challenge to work with the MCP remote secured server.

```python
"""
Secure MCP Client - Challenge 09

This client demonstrates how to connect to a remote MCP server
that requires API key authentication.
"""

import asyncio
import os
from contextlib import AsyncExitStack
from agent_framework import ChatAgent, MCPSSETool
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

load_dotenv()


async def main():
    """Connect to secure remote MCP server and use tools."""

    # Configuration
    remote_server_url = os.getenv("MCP_SERVER_URL", "http://localhost:5000")
    api_key = os.getenv("API_KEY", "your-secure-api-key-change-this")

    # Azure OpenAI configuration
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    openai_key = os.getenv("AZURE_OPENAI_API_KEY")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")

    if not all([endpoint, openai_key, deployment_name]):
        raise ValueError("Missing Azure OpenAI configuration")

    async with AsyncExitStack() as stack:
        # Create chat client
        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=openai_key,
            deployment_name=deployment_name,
            api_version="latest"
        )

        # Create MCP tool with API key in headers
        mcp_tool = MCPSSETool(
            name="WeatherMCP",
            url=remote_server_url,
            extra_headers={
                "X-API-Key": api_key
            }
        )

        # Create agent with MCP tools
        agent = await stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name="WeatherAgent",
                instructions="You are a helpful weather assistant with access to weather tools.",
                tools=[mcp_tool]
            )
        )

        # Example: Query weather for New York
        query = "What's the weather forecast for New York?"
        print(f"Query: {query}\n")

        async for update in agent.run_stream(query):
            if update.text:
                print(update.text, end="", flush=True)
        print()


if __name__ == "__main__":
    asyncio.run(main())
```

### Task 5: Verify secure communication between MCP Client and Server

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