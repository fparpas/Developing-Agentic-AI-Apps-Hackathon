# Secure Weather MCP Server

A secure Model Context Protocol (MCP) server implementation that provides weather data through authenticated HTTP endpoints.

## Features

- **API Key Authentication**: Secure access using API keys
- **RESTful MCP Endpoints**: HTTP-based MCP tool execution
- **Weather Tools**: Current weather and forecast functionality
- **Admin Interface**: API key management endpoints
- **Security Headers**: Production-ready security configuration
- **Swagger Documentation**: Interactive API documentation
- **Comprehensive Logging**: Request tracking and error monitoring

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code

### Running the Server

1. Navigate to the project directory:
   ```bash
   cd SecureWeatherMcpServer
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The server will start on `https://localhost:7043` (or `http://localhost:5043`)

4. Access the Swagger UI at: `https://localhost:7043`

### Default API Key

The server includes a demo API key for testing:
- **API Key**: `sk-demo-weather-api-key-12345`
- **Permissions**: `weather:read`, `tools:list`

## API Endpoints

### MCP Endpoints (Require Authentication)

#### List Available Tools
```http
POST /api/mcp/tools/list
X-API-Key: sk-demo-weather-api-key-12345
```

#### Execute Tool
```http
POST /api/mcp/tools/call
X-API-Key: sk-demo-weather-api-key-12345
Content-Type: application/json

{
  "name": "get_current_weather",
  "arguments": {
    "location": "London, UK",
    "unit": "celsius"
  }
}
```

### Admin Endpoints (Require Authentication)

#### Get API Keys
```http
GET /api/admin/apikeys
X-API-Key: sk-demo-weather-api-key-12345
```

#### Create API Key
```http
POST /api/admin/apikeys
X-API-Key: sk-demo-weather-api-key-12345
Content-Type: application/json

{
  "name": "My App Key",
  "permissions": ["weather:read", "tools:list"]
}
```

#### Revoke API Key
```http
DELETE /api/admin/apikeys/{keyId}
X-API-Key: sk-demo-weather-api-key-12345
```

#### Health Check (No Authentication Required)
```http
GET /api/admin/health
```

## Available Weather Tools

### get_current_weather
Get current weather conditions for a location.

**Parameters:**
- `location` (required): City and country (e.g., "London, UK")
- `unit` (optional): "celsius" or "fahrenheit" (default: "celsius")

### get_weather_forecast
Get weather forecast for a location.

**Parameters:**
- `location` (required): City and country (e.g., "London, UK")
- `days` (optional): Number of forecast days 1-7 (default: 5)

## Security Features

### API Key Authentication
- Custom authentication handler validates API keys
- Keys are hashed and stored securely
- Support for permissions-based access control

### Security Headers
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- X-XSS-Protection: 1; mode=block
- Strict-Transport-Security: max-age=31536000
- Referrer-Policy: strict-origin-when-cross-origin

### CORS Configuration
- Configured for MCP client access
- Allows cross-origin requests for tool execution

## Development

### Project Structure
```
SecureWeatherMcpServer/
├── Authentication/
│   └── ApiKeyAuthenticationHandler.cs
├── Controllers/
│   ├── AdminController.cs
│   └── McpController.cs
├── Models/
│   └── ApiKeyModels.cs
├── Services/
│   ├── ApiKeyService.cs
│   └── WeatherService.cs
├── Program.cs
└── appsettings.json
```

### Building and Testing

```bash
# Build the project
dotnet build

# Run tests
dotnet test

# Publish for production
dotnet publish -c Release
```

## Usage with MCP Clients

This server can be used with any MCP client that supports HTTP transport. Configure your client to:

1. Use HTTP transport mode
2. Set the base URL to `https://localhost:7043/api/mcp`
3. Include the API key in the `X-API-Key` header
4. Use the `/tools/list` and `/tools/call` endpoints

## Production Deployment

### Environment Configuration
- Configure logging levels in `appsettings.Production.json`
- Set up proper SSL certificates
- Configure external weather API integration
- Implement persistent storage for API keys
- Set up monitoring and alerting

### Security Considerations
- Use strong API keys in production
- Implement rate limiting
- Set up proper CORS policies
- Enable HTTPS only
- Implement audit logging
- Use secure key storage (Azure Key Vault, etc.)

## License

This project is part of the Developing Agentic AI Apps Hackathon challenge materials.
