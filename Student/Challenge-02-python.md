# Challenge 02 - Python - Build your first MCP server

 [< Previous Challenge](./Challenge-01.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-03-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-02-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-02-python.md)

## Introduction

In this challenge, you will build and run a minimal Model Context Protocol (MCP) server locally, wire it into GitHub Copilot Chat in Visual Studio Code, and invoke the new tools and resources from chat.

## Concepts

The Model Context Protocol (MCP) is an open standard for connecting AI language models (LLMs) with external tools, resources, and prompts. It enables LLMs to extend their capabilities by calling out to specialized services, enhancing their functionality beyond text generation.

MCP servers provide tools, resources, and prompts over a standard transport. An IDE agent (like Copilot Chat) connects to your server, lists capabilities, and calls your tools with JSON inputs, receiving structured outputs.

- **Transport:** Most commonly, MCP servers run as local processes launched by VS Code or Copilot, communicating via stdio. Alternatively, MCP servers can be hosted remotely and accessed over a network, allowing multiple users or agents to connect.
- **Local vs Remote Servers:**
    - *Local servers* run on your machine, providing fast, direct integration with your IDE and access to local files or resources.
    - *Remote servers* are hosted elsewhere (e.g. in Azure Functions, Azure Container Apps), enabling centralized management, scalability, and access from different locations or users.
- **Capabilities:** Tools (functions you expose), resources (read-only data), prompts (templated guidance).
    - Tools:
        These are callable functions that your system makes available for use. They perform specific actions, such as fetching data, processing information, or interacting with external services. Think of them as the "active" capabilities your application provides.
    - Resources:
        These are datasets or information that users or systems can access but not modify. Resources might include documentation, configuration files, or reference tables. They provide valuable context or support without allowing changes.
    - Prompts:
        Prompts are pre-defined templates or instructions designed to guide users or systems in performing tasks. They help standardize interactions, ensuring consistency and clarity. For example, a prompt might be a template for asking a user to input specific information.

This architecture lets you choose between local development convenience and remote deployment flexibility, depending on your needs.

## Description

In this challenge, you will build a simple MCP weather server and connect it to a host (Visual Studio Code or Claude for Desktop).

Many LLMs do not currently have the ability to fetch real-time forecasts and severe weather alerts. Let’s use MCP to solve that.

You will build a server that exposes two tools: `get_alerts` and `get_forecast`, then connect the server to an MCP host (Visual Studio Code or Claude for Desktop).

> ℹ️ Servers can connect to any client. We’ve chosen Visual Studio Code or Claude Desktop here for simplicity, but you could also connect to other clients like Copilot Chat in JetBrains IDEs or even build your own client.

### Task 1: Set up your environment

**For faster dependency management, consider using `uv`:** [`uv` is an extremely fast Python package installer and resolver](https://docs.astral.sh/uv/). It's significantly faster than `pip` (10-100x in many cases) and handles dependency resolution more efficiently. You can install it from https://docs.astral.sh/uv/getting-started/installation/.

Use the official MCP Python quickstart as your base: https://modelcontextprotocol.io/quickstart/server

1. Create a project directory and install dependencies

**Using `uv` (recommended for performance):**
```bash
# Create a new directory for the project
mkdir weather_mcp_server
cd weather_mcp_server

# Create a virtual environment
uv venv .venv

# Activate the virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install the MCP SDK and dependencies
uv pip install "mcp[cli]" httpx
```

**Or using standard `pip`:**
```bash
# Create a new directory for the project
mkdir weather_mcp_server
cd weather_mcp_server

# Create a virtual environment
python -m venv .venv

# Activate the virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install the MCP SDK and dependencies
pip install "mcp[cli]" httpx
```

### Task 2: Create your MCP server (Python)

Create a file named `weather.py` in your project directory with the following code. This sets up a basic MCP server that uses the FastMCP pattern for automatic tool discovery:
```python
from typing import Any
import httpx
from mcp.server.fastmcp import FastMCP
import sys

# Initialize FastMCP server - this automatically handles stdio transport
mcp = FastMCP("weather")

# Constants for the National Weather Service API
NWS_API_BASE = "https://api.weather.gov"
USER_AGENT = "weather-app/1.0"


async def make_nws_request(url: str) -> dict[str, Any] | None:
    """Make a request to the NWS API with proper error handling.

    This helper function handles HTTP requests to the National Weather Service
    API with proper headers and timeout configuration.
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
    """Format an alert feature into a readable string.

    Takes a GeoJSON feature from the NWS API and extracts key
    alert information for display to the user.
    """
    props = feature["properties"]
    return f"""
Event: {props.get('event', 'Unknown')}
Area: {props.get('areaDesc', 'Unknown')}
Severity: {props.get('severity', 'Unknown')}
Description: {props.get('description', 'No description available')}
Instructions: {props.get('instruction', 'No specific instructions provided')}
"""
```

### Task 3: Add weather tools to your MCP server

Add the following tools to your server:

- `get_forecast`: Returns a short forecast by latitude/longitude using api.weather.gov (NWS).
- `get_alerts`: Returns active severe weather alerts for a US state using api.weather.gov (NWS).

> ℹ️ These weather tools integrate with the National Weather Service API. It provides real-time weather updates and alerts for US locations only.

Add these tool functions to your `weather.py` file (after the helper functions from Task 2):

```python
@mcp.tool()
async def get_alerts(state: str) -> str:
    """Get weather alerts for a US state.

    This tool queries the National Weather Service API for active severe weather
    alerts in the specified US state.

    Args:
        state: Two-letter US state code (e.g. CA, NY, WA, TX)
    """
    # Construct URL to fetch alerts for the specified state
    url = f"{NWS_API_BASE}/alerts/active/area/{state}"
    data = await make_nws_request(url)

    # Handle API errors or missing data
    if not data or "features" not in data:
        return "Unable to fetch alerts or no alerts found."

    # Return appropriate message if no active alerts
    if not data["features"]:
        return "No active alerts for this state."

    # Format and join all alerts with a separator
    alerts = [format_alert(feature) for feature in data["features"]]
    return "\n---\n".join(alerts)


@mcp.tool()
async def get_forecast(latitude: float, longitude: float) -> str:
    """Get weather forecast for a location.

    This tool retrieves a detailed weather forecast for a specific geographic
    location using the National Weather Service API.

    Args:
        latitude: Latitude of the location
        longitude: Longitude of the location
    """
    # First, get the forecast grid endpoint for the given coordinates
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

    # Fetch the actual forecast data
    forecast_data = await make_nws_request(forecast_url)
    if not forecast_data:
        return "Unable to fetch detailed forecast."

    # Format the forecast periods into readable text
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
```

### Task 4: Initialize and run the server

Finally, add this code to the end of your `weather.py` file to initialize and run the server:

```python
def main():
    """Initialize and run the MCP server.

    This starts the server listening on stdio, which allows MCP hosts
    (like Claude Desktop or VS Code) to communicate with it.
    """
    print('[INFO] MCP server is up.', file=sys.stderr)
    mcp.run(transport='stdio')


if __name__ == "__main__":
    main()
```

### Task 5: Run and validate locally

From your project folder:
```bash
python weather.py
```

This starts the server and listens for incoming requests on standard input/output.


### Task 6: Connect to an MCP host

Option A: Visual Studio Code (GitHub Copilot Chat)
- Create a `.vscode/mcp.json` file in your workspace.
- Add a server entry:

```json
{
	"servers": {
		"Challenge-02-stdio-MCP-Server": {
			"type": "stdio",
			"command": "/absolute/path/to/.venv/bin/python",
			"args": [
				"/absolute/path/to/weather.py"
			]
		}
	}
}
```

- Reload VS Code. In Copilot Chat, use `/tools` to see your server and try:
    - `get_forecast` with latitude/longitude (e.g., 47.6062, -122.3321)
    - `get_alerts` with a two-letter state (e.g., WA)

Option B: Claude Desktop
- Follow the Claude MCP config [guide:](https://modelcontextprotocol.io/quickstart/server#testing-your-server-with-claude-for-desktop-5)
- Add to your `claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "weather": {
      "command": "/absolute/path/to/.venv/bin/python",
      "args": ["/absolute/path/to/weather.py"]
    }
  }
}
```

### Task 7: Use MCP Inspector for testing and debugging Model Context Protocol servers

[MCP Inspector](https://modelcontextprotocol.io/legacy/tools/inspector) is an interactive developer tool for testing and debugging MCP servers. It lets you start/attach servers, call tools with JSON inputs, inspect requests/responses, and view logs.

1. Prerequisites
    - Node.js 18+ installed
    - Your weather MCP server runs locally

2. Start the Inspector
```bash
npx @modelcontextprotocol/inspector
```
The Inspector opens in your browser (or prints a local URL). Keep the terminal open.

3. Configure a server via the UI
     - In the Inspector, add the MCP server
     - Choose `stdio`
     - Command: `/absolute/path/to/.venv/bin/python` (path to python binary from your venv)
     - Args: `/absolute/path/to/weather.py`
     - Use the UI to:
         - List tools (`get_forecast`, `get_alerts`)
         - Invoke a tool and provide JSON input, for example:
             - `get_alerts`: `{ "state": "WA" }`
             - `get_forecast`: `{ "latitude": 47.6062, "longitude": -122.3321 }`

## Success Criteria

- ✅ A Python MCP server runs locally over stdio.
- ✅ The server lists two tools: `get_forecast` and `get_alerts`.
- ✅ Invoking `get_forecast` returns weather forecast info for the given latitude/longitude.
- ✅ Invoking `get_alerts` returns zero or more active alerts for the specified US state.
- ✅ Tools are visible and callable from your chosen MCP host (VS Code Copilot Chat or Claude Desktop).
- ✅ Validated with MCP Inspector: server connects via stdio, tools (`get_forecast`, `get_alerts`) invoke successfully, and requests/responses are visible without errors.
- ✅ The user gets a response to the prompt "Get the weather in Sacramento" when using the MCP tools in VS Code Copilot Chat or Claude Desktop.

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [VS Code MCP Tools](https://code.visualstudio.com/docs/copilot/customization/mcp-servers#_use-mcp-tools-in-agent-mode)
- [MCP SDK Documentation](https://modelcontextprotocol.io/docs/sdk)
- [Quickstart (server)](https://modelcontextprotocol.io/quickstart/server#c%23)
- [VS Code MCP Servers](https://code.visualstudio.com/mcp)
- [GitHub Copilot in VS Code](https://code.visualstudio.com/docs/editor/github-copilot)
- [Weather.gov API](https://www.weather.gov/documentation/services-web-api)
- [MCP Inspector](https://modelcontextprotocol.io/legacy/tools/inspector)
- [Deep Dive: Understanding MCP Client-Server Communication with Agent and LLM](https://medium.com/@jamestang/deep-dive-understanding-mcp-client-server-communication-with-agent-and-llm-aa4782a65991)
