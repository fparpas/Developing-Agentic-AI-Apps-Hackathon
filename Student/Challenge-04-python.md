# Challenge 04 - Python - Host MCP Remote Servers on ACA or Azure Functions

 [< Previous Challenge](./Challenge-03-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-05-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-04-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-04-python.md)

## Introduction

In this challenge, you'll build and deploy a remote Model Context Protocol (MCP) server using Python that can be accessed over HTTP. Unlike the previous challenges where the MCP server ran locally with stdio transport, this challenge focuses on creating a web-accessible MCP server running in Azure.

You'll start with an incomplete `weather_remote_server.py` project, complete the implementation, and deploy it to either Azure Container Apps (ACA) or Azure Functions. Both deployment options are provided, giving you flexibility to choose the Azure service that best fits your needs.

## Key Concepts

Understanding these core concepts will help you succeed in this challenge.

### MCP Transport: From Local to Remote

**Previous Challenges (stdio):**
- MCP server and client ran on the same machine
- Direct communication through input/output streams
- Perfect for local development and testing

**This Challenge (HTTP):**
- MCP server runs in the cloud, accessible from anywhere
- Clients connect over the internet using standard HTTP web protocols
- Enables multiple clients to use the same server simultaneously

### Azure Deployment: Two Simple Options

**Azure Container Apps**
- Best for: Apps that need flexibility and control
- Scaling: Automatically adjusts to traffic, scales to zero when idle
- Cost: Pay only for what you use
- Think of it as: A smart hosting service for containerized apps

**Azure Functions**
- Best for: Simple APIs with minimal management
- Scaling: Handles everything automatically
- Cost: Extremely low cost for light usage
- Think of it as: Run your code only when needed

**How to Choose:**
- **Container Apps**: If you want more control and expect regular traffic
- **Functions**: If you want maximum simplicity and minimal cost

## Description

In this challenge, you'll complete and deploy a **WeatherRemoteMcpServer** that:
- Implements the Model Context Protocol over HTTP transport
- Provides weather forecasting and alert tools
- Runs as a containerized web application
- Can be accessed remotely by MCP clients
- Integrates with the National Weather Service API

You'll build a weather MCP server that runs in Azure instead of locally, can be accessed by remote clients from anywhere, provides real-time weather forecasts and alerts, and scales automatically based on usage demand.

Moving from local to remote MCP servers unlocks significant advantages: multiple AI agents can use your server simultaneously, the service is always available without requiring local execution, it handles many requests automatically without your intervention, and follows production-ready deployment patterns used in real-world applications.

> **📝 Note:** For simplicity, this challenge does not implement authentication or authorization. The MCP server will be publicly accessible without security restrictions. Authentication and authorization patterns for production MCP servers will be covered in upcoming challenges.

This challenge consists of three main tasks that build upon each other:

# Task 1: Complete the MCP Server Implementation
**Goal:** Get the WeatherRemoteMcpServer running with HTTP transport

**What you'll do:**
- Complete the incomplete `weather_remote_server.py` file
- Configure MCP server with HTTP transport using FastAPI instead of stdio
- Implement the weather tools: `get_forecast` and `get_alerts`
- Ensure the application runs on port 8000 and handles MCP protocol requests
- Add FastAPI endpoints for health checks and server information

## Project Structure

Your project starting point is located in the `Coach/Solutions/Challenge-04/python/` directory:

```
📁 Coach/Solutions/Challenge-04/python/
├── 📄 weather_remote_server.py         # ⚠️  INCOMPLETE - You need to complete this
├── 📄 requirements.txt                 # Project dependencies (provided)
├── 📄 Dockerfile                       # Container Apps Dockerfile (provided)
└── 📄 README.md                        # Deployment instructions (provided)
```

## What You Need to Complete

The `weather_remote_server.py` file is partially complete. You'll need to:

1. Since we're going to deploy this MCP server to Azure, we need to change the transport type to HTTP. It's on you to find out how.
2. Implement the health endpoint so Azure Container Apps can check container health.
3. Run the server. Ensure it starts on port 8000/TCP.

## Starter Code

`requirements.txt`:
```plaintext
# FastAPI + MCP Remote Server Requirements
# For deployment on Azure Container Apps or Azure Functions

# Web Framework
fastapi>=0.133.0
uvicorn[standard]>=0.41.0

# MCP SDK
mcp[cli]>=1.26.0

# HTTP Client
httpx>=0.28.1

# For Azure deployment
azure-identity>=1.25.2
azure-functions>=1.24.0  # Only needed for Azure Functions deployment
```

Here's the incomplete `weather_remote_server.py` that you need to complete:

```python
"""
Remote MCP Server for Azure Deployment - Challenge 04 Solution

This module implements an MCP server using FastAPI for HTTP transport,
making it suitable for deployment on Azure Container Apps or Azure Functions.

The server is containerized and runs as a web service, allowing remote
clients to access weather tools via HTTP instead of stdio.
"""

from typing import Any
import httpx
import uvicorn
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

# Create the MCP Streamable HTTP sub-application
mcp_app = mcp.streamable_http_app()

@asynccontextmanager
async def lifespan(app):
    """Run the MCP server's lifespan within FastAPI's lifespan."""
    async with mcp_app.router.lifespan_context(mcp_app):
        yield

# Create FastAPI application
app = FastAPI(title="Weather MCP Server", version="1.0.0", lifespan=lifespan)

# TODO: Implement health check endpoint
# ❌ Create a GET /health endpoint
#
# @app.get("/health")
# async def health_check():
#   Return: dictionary with {"status": "healthy", "service": "weather-mcp-server"}


# TODO (Optional): Implement root information endpoint
# ❌ Create a GET / endpoint
#
# @app.get("/")
# async def root():
#   Return: dictionary with server information including:
#     - name: "Weather MCP Server"
#     - version: "1.0.0"
#     - description: "MCP server providing weather forecasts and alerts"
#     - tools: list of available tools ["get_forecast", "get_alerts"]
#     - health_check: "/health"

# Mount the MCP Streamable HTTP transport at /mcp
app.mount("/", mcp_app)

if __name__ == "__main__":
    print("\nStarting Weather MCP Server on port 8000...\n")
    # uvicorn runs the server on port 8000 (Azure will expose it on port 80)
    # Use 0.0.0.0 to accept connections from any interface, not just localhost
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=8000,
        log_level="info"
    )
```

If you get stuck, you can refer to the solution in the Coach directory.

### Hint for Port Configuration

The application should run on port 8000 locally. When deployed to Azure, the platform will handle routing to your container's port.

## Testing Locally

Once you've completed the implementation, test it locally:

```bash
# Create virtual environment
python -m venv .venv

# or with uv (https://docs.astral.sh/uv/)
uv venv .venv

# Activate virtual environment
source .venv/bin/activate   # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# or with uv
uv pip install -r requirements.txt

# Run the server
python weather_remote_server.py
```

Then visit `http://localhost:8000` to see the server information and `http://localhost:8000/health to check liveliness.

# Task 2: Deploy to Azure
**Goal:** Get your MCP server running in the cloud using predefined deployment scripts

**Description:**
Once your MCP server is working locally, it's time to deploy it to Azure so it can be accessed remotely over the internet. We've provided complete automation scripts and detailed README files that walk you through the entire deployment process step-by-step.

**Choose your deployment method:**
- **Option A:** Azure Container Apps (recommended for flexibility)
  - Instructions: [Azure Container Apps README](./Resources/Challenge-04/README-ACA.md)
- **Option B:** Azure Functions (recommended for simplicity)
  - Instructions: [Azure Functions README](./Resources/Challenge-04/README-Functions.md)

**What you'll do:**
- Choose your preferred deployment option and read the corresponding README file
- Follow the deployment instructions to create and deploy all Azure resources
- Verify your deployment is accessible via the provided URL

# Task 3: Test with MCP Inspector
Verify your remote MCP server works with real MCP clients (using Streamable HTTP transport)

**What you'll do:**
- Run MCP Inspector tool. Connect to `/mcp` endpoint via Streamable HTTP transport, then under Tools, test with these latitude/longitude coordinates for New York: 40.7128, -74.0060.
- Connect to your deployed Azure server over HTTP, make a GET request to / and /health

## Success Criteria

- ✅ Complete the MCP server implementation with HTTP transport
- ✅ Application runs without errors on port 8000 locally
- ✅ Server responds to HTTP requests on `/`, `/health`, and MCP protocol endpoints
- ✅ Both `get_forecast` and `get_alerts` tools are properly decorated and functional
- ✅ Tools correctly fetch and format data from the National Weather Service API
- ✅ Successfully deploy to either Azure Container Apps or Azure Functions
- ✅ Application is accessible via public Azure URL
- ✅ Basic connectivity test confirms MCP server is running in the cloud
- ✅ MCP Inspector successfully connects to your deployed server (Connect to /mcp endpoint via Streamable HTTP transport)
- ✅ Weather tools (`get_forecast` and `get_alerts`) are visible and functional in MCP Inspector
- ✅ Tools return real data and work with various locations (i.e. Use 40.7128, -74.0060 for New York)
- ✅ Complete end-to-end remote MCP server functionality demonstration


## Learning Resources

### Key Concepts
- **FastAPI**: Modern Python web framework for building APIs quickly
- **HTTP Transport**: Making MCP accessible over the internet using standard web protocols
- **Async/Await**: Python's approach to handling concurrent operations
- **Containerization**: Packaging your application for consistent deployment across environments

### Documentation
- 📖 [Azure Container Apps README](./Resources/Challenge-04/README-ACA.md)
- 📖 [Azure Functions README](./Resources/Challenge-04/README-Functions.md)

### MCP Documentation
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP Python SDK Documentation](https://github.com/modelcontextprotocol/python-sdk)
- [MCP FastMCP Quick Start](https://modelcontextprotocol.io/tutorials/server/python)

### Python Libraries
- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [httpx - HTTP Client for Python](https://www.python-httpx.org/)
- [Uvicorn - ASGI Server](https://www.uvicorn.org/)

### Azure Services
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)