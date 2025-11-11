# Weather MCP Server - Challenge 02 Solution (Python)

This is the Python solution for Challenge 02 of the Developing Agentic AI Apps Hackathon. This project implements a Model Context Protocol (MCP) server that provides weather forecasting and weather alert tools using the National Weather Service API.

## Features

The MCP server provides two main tools:

1. **get_forecast**: Returns a detailed weather forecast for a given latitude and longitude using the National Weather Service API
2. **get_alerts**: Returns active severe weather alerts for a specified US state

## Project Structure

```
weather_mcp_server/
├── weather.py              # Main MCP server implementation
├── requirements.txt        # Python dependencies
└── .venv/                 # Virtual environment (created during setup)
```

## Dependencies

- **mcp[cli]** (1.2.0+): Core MCP SDK for Python with CLI tools
- **httpx** (0.24.0+): Async HTTP client for API requests

## Setup and Running the Server

### 1. Create Virtual Environment and Install Dependencies

**For faster dependency management, consider using `uv`:** [`uv` is an extremely fast Python package installer and resolver](https://docs.astral.sh/uv/). It's significantly faster than `pip` (10-100x in many cases) and handles dependency resolution more efficiently. You can install it from https://docs.astral.sh/uv/getting-started/installation/.

**Using `uv` (recommended for performance):**
```bash
# Create project directory
mkdir weather_mcp_server
cd weather_mcp_server

# Create virtual environment
uv venv .venv

# Activate virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install dependencies
uv pip install -r requirements.txt
```

**Or using standard `pip`:**
```bash
# Create project directory
mkdir weather_mcp_server
cd weather_mcp_server

# Create virtual environment
python -m venv .venv

# Activate virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt
```

### 2. Run the Server

```bash
python weather.py
```

The server communicates via standard input/output (stdio) and is designed to be integrated with MCP hosts like Visual Studio Code with GitHub Copilot Chat or Claude Desktop.

## Connecting to VS Code

To connect this server to VS Code with GitHub Copilot Chat:

1. Open your VS Code command pallette (CTRL-Shift-P / Command-Shift-P): search for "MCP: Add Server".

2. Add a server entry with:
   - **command**: `/absolute/path/to/.venv/bin/python`
   - **args**: `["/absolute/path/to/weather.py"]`
   - **transport**: `stdio`

3. Reload VS Code and test with `/tools` in Copilot Chat

## Connecting to Claude Desktop

To use with Claude Desktop:

1. Open your Claude Desktop configuration file:
   ```
   ~/Library/Application Support/Claude/claude_desktop_config.json
   ```

2. Add the weather server to the mcpServers section:
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

3. Save the file and restart Claude Desktop

## Tool Usage Examples

### Get Forecast
Query the forecast for a specific location using latitude and longitude:
```json
{
  "latitude": 47.6062,
  "longitude": -122.3321
}
```
This returns the next 5 forecast periods for Seattle, WA.

### Get Alerts
Query active severe weather alerts for a US state:
```json
{
  "state": "WA"
}
```
This returns all active severe weather alerts for Washington state.

## Testing with MCP Inspector

You can test the server using the MCP Inspector:

```bash
npx @modelcontextprotocol/inspector
```

Then configure a server with:
- **Command**: `/absolute/path/to/.venv/bin/python`
- **Args**: `/absolute/path/to/weather.py`

The Inspector allows you to:
- List available tools (get_forecast, get_alerts)
- Call tools with JSON inputs
- View request/response messages
- Debug any issues

## API Data Sources

- **Forecasts**: Uses the National Weather Service API (api.weather.gov)
- **Alerts**: Uses the National Weather Service alerts API for active severe weather alerts

## Implementation Notes

### Architecture

The server uses the FastMCP pattern, which automatically handles:
- Tool discovery and registration based on decorated functions
- JSON-RPC message handling
- Stdio transport for communication with MCP hosts

### Key Features

- **Async/Await**: All API calls are asynchronous for better performance
- **Error Handling**: Graceful error handling with informative messages
- **Type Hints**: Full Python type hints for better IDE support and code clarity
- **Documentation**: Comprehensive docstrings on all functions

### Tool Implementation

Each tool is implemented as an async function decorated with `@mcp.tool()`:

1. **get_alerts**:
   - Validates the state parameter
   - Calls the NWS alerts API
   - Formats each alert with key information
   - Returns formatted text or an error message

2. **get_forecast**:
   - Takes latitude and longitude coordinates
   - Makes a two-step API call (first to get the forecast URL, then to get the forecast)
   - Extracts the next 5 periods from the forecast data
   - Returns formatted forecast text with temperature, wind, and conditions

## Success Criteria Met

✅ A Python MCP server runs locally over stdio
✅ The server lists two tools: get_forecast and get_alerts
✅ get_forecast returns detailed forecast info for given lat/lon
✅ get_alerts returns active alerts for given US state
✅ Tools are discoverable and callable from MCP hosts
✅ Compatible with MCP Inspector for testing and debugging
✅ Code is clean, well-documented, and follows Python best practices

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [MCP Python Documentation](https://modelcontextprotocol.io/docs/sdk/python)
- [MCP Server Quickstart](https://modelcontextprotocol.io/quickstart/server)
- [MCP Inspector Guide](https://modelcontextprotocol.io/legacy/tools/inspector)
- [VS Code MCP Tools](https://code.visualstudio.com/docs/copilot/customization/mcp-servers#_use-mcp-tools-in-agent-mode)
- [National Weather Service API](https://www.weather.gov/documentation/services-web-api)
- [FastMCP Documentation](https://modelcontextprotocol.io/docs/sdk/python/fastmcp)
