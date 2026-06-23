# Testing SecureWeatherMcpServer with MCP Inspector

This guide walks you through testing your secure MCP server using the official Model Context Protocol Inspector tool.

## Prerequisites

- Node.js installed (version 16 or higher)
- Your SecureWeatherMcpServer running locally or deployed to Azure
- API key for authentication (`SuperSecureSecretUsedAsApiKey`)

## Step 1: Start Your MCP Server

### Option A: Run Locally
```powershell
# Navigate to the project directory
cd "c:\Users\phanisparpas.MIDDLEEAST\source\repos\Developing-Agentic-AI-Apps-Hackathon\Coach\Solutions\Challenge-06\SecureWeatherMcpServer"

# Run the server
dotnet run
```

The server will start on `http://localhost:5000` or `https://localhost:5001`

### Option B: Use Deployed Azure Server
If you have deployed to Azure, use your Azure URL (e.g., `https://your-app.azurewebsites.net`)

## Step 2: Install and Run MCP Inspector

Open a new terminal/command prompt and run:

```powershell
# Install and run the MCP Inspector
npx @modelcontextprotocol/inspector dotnet run --project "C:\Users\phanisparpas.MIDDLEEAST\source\repos\Developing-Agentic-AI-Apps-Hackathon\Coach\Solutions\Challenge-06\SecureWeatherMcpServer\SecureWeatherMcpServer.csproj"
```

## Step 3: Connect to Your Secure MCP Server

1. **Open your browser** and navigate to the MCP Inspector interface (usually `http://localhost:5173`)

2. **Configure the connection:**
   - **Transport Type**: Select `HTTP`
   - **Server URL**: Enter your server URL:
     - Local: `http://localhost:5000/mcp` or `https://localhost:5001/mcp`
     - Azure: `https://your-app.azurewebsites.net/mcp`

3. **Add Authentication Headers:**
   Click on "Headers" or "Authentication" section and add:
   ```
   Header Name: X-API-KEY
   Header Value: SuperSecureSecretUsedAsApiKey
   ```

4. **Click "Connect"**

## Step 4: Test the Weather Tools

Once connected, you should see the available tools:

### Available Tools:
- `get_forecast` - Get weather forecast for a location
- `get_alerts` - Get weather alerts for a US state

### Test Examples:

#### Test Weather Forecast:
```json
{
  "name": "get_forecast",
  "arguments": {
    "latitude": 40.7128,
    "longitude": -74.0060
  }
}
```

#### Test Weather Alerts:
```json
{
  "name": "get_alerts",
  "arguments": {
    "state": "NY"
  }
}
```

## Step 5: Verify Results

✅ **Success Indicators:**
- Inspector connects without authentication errors
- Both weather tools are visible in the tools list
- Tool calls return real weather data from the National Weather Service
- No authentication or authorization errors in the server logs

❌ **Troubleshooting:**

### Authentication Issues:
- **Error: "Unauthorized"** → Check that the API key header is correctly set
- **Error: "Forbidden"** → Verify the API key value matches exactly: `SuperSecureSecretUsedAsApiKey`

### Connection Issues:
- **Error: "Connection failed"** → Ensure the server is running and accessible
- **Error: "CORS"** → Check that your server allows requests from the Inspector origin

### Tool Issues:
- **No tools visible** → Check server logs for MCP registration errors
- **Tool execution fails** → Verify the National Weather Service API is accessible

## Authentication Flow

Your secure MCP server implements API key authentication:

1. **Client Request** → Must include `X-API-KEY` header
2. **Server Validation** → Checks API key against configured value
3. **Access Granted/Denied** → Based on authentication result

## Security Notes

⚠️ **Important Security Considerations:**

- The API key used (`SuperSecureSecretUsedAsApiKey`) is for demonstration purposes only
- In production, use strong, randomly generated API keys
- Consider implementing:
  - Rate limiting
  - API key rotation
  - More granular authorization
  - HTTPS-only communication

## Next Steps

After successful testing with MCP Inspector:

1. **Try different coordinates** for weather forecasts
2. **Test various US states** for weather alerts
3. **Monitor server logs** to understand the authentication flow
4. **Experiment with invalid API keys** to see security behavior

## Useful Commands

```powershell
# Check if your server is running
curl http://localhost:5000

# Test authentication directly
curl -H "X-API-KEY: SuperSecureSecretUsedAsApiKey" http://localhost:5000/mcp

# View server logs
# (Check your terminal where you ran 'dotnet run')
```

## Documentation Links

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP Inspector Documentation](https://github.com/modelcontextprotocol/inspector)
- [National Weather Service API](https://www.weather.gov/documentation/services-web-api)
