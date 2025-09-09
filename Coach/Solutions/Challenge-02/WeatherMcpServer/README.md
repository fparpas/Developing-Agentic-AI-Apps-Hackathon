# Weather MCP Server - Challenge 02 Solution

This is the solution for Challenge 02 of the Developing Agentic AI Apps Hackathon. This project implements a Model Context Protocol (MCP) server that provides weather forecasting and weather alert tools.

## Features

The MCP server provides two main tools:

1. **get_forecast**: Returns a detailed weather forecast for a given latitude and longitude using the National Weather Service API
2. **get_alerts**: Returns active severe weather alerts for a specified US state

## Project Structure

```
WeatherMcpServer/
├── Program.cs              # Main entry point with MCP server configuration
├── HttpClientExt.cs        # Extension methods for HttpClient JSON handling
├── Tools/
│   └── WeatherTools.cs     # Weather tool implementations
└── WeatherMcpServer.csproj # Project file with dependencies
```

## Dependencies

- **ModelContextProtocol** (prerelease): Core MCP SDK for .NET
- **Microsoft.Extensions.Hosting**: Hosting infrastructure for the console application

## Running the Server

1. **From Command Line:**
   ```bash
   cd WeatherMcpServer
   dotnet run
   ```

2. **From Project File:**
   ```bash
   dotnet run --project WeatherMcpServer/WeatherMcpServer.csproj
   ```

The server communicates via standard input/output (stdio) and is designed to be integrated with MCP hosts like Visual Studio Code with GitHub Copilot Chat or Claude Desktop.

## Connecting to VS Code

To connect this server to VS Code with GitHub Copilot Chat:

1. Configure the MCP server in your VS Code settings
2. Add a server entry with:
   - **command**: `dotnet`
   - **args**: `["run", "--project", "<absolute-path-to>/WeatherMcpServer/WeatherMcpServer.csproj"]`
   - **transport**: `stdio`

## Tool Usage Examples

### Get Forecast
```json
{
  "latitude": 47.6062,
  "longitude": -122.3321
}
```

### Get Alerts
```json
{
  "state": "WA"
}
```

## Testing with MCP Inspector

You can test the server using the MCP Inspector:

```bash
npx @modelcontextprotocol/inspector
```

Then configure a server with:
- **Command**: `dotnet`
- **Args**: `run`, `--project`, `<absolute-path-to>/WeatherMcpServer/WeatherMcpServer.csproj`

## API Data Sources

- **Forecasts**: Uses the National Weather Service API (api.weather.gov)
- **Alerts**: Uses the National Weather Service alerts API for active severe weather alerts

## Implementation Notes

- The server uses HttpClient with proper User-Agent headers for API compliance
- JSON responses are parsed using System.Text.Json for performance
- Error handling includes proper API response validation
- Tools are decorated with MCP attributes for automatic discovery
- The application uses the .NET hosting model for proper lifecycle management

## Success Criteria Met

✅ A .NET MCP server runs locally over stdio  
✅ The server lists two tools: get_forecast and get_alerts  
✅ get_forecast returns detailed forecast info for given lat/lon  
✅ get_alerts returns active alerts for given US state  
✅ Tools are discoverable and callable from MCP hosts  
✅ Compatible with MCP Inspector for testing and debugging
