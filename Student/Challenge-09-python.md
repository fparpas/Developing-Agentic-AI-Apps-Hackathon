# Challenge 09 - Python - Secure your MCP remote server using an API key

[< Previous Challenge](./Challenge-08-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-10-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-09-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-09-python.md)

![](https://img.shields.io/badge/Challenge%20Under%20Development-red)

## Introduction

In previous challenges, you built MCP servers and clients that communicate over stdio (standard input/output). While this works well for local development, production scenarios often require remote MCP servers that can be accessed over HTTP/HTTPS from multiple clients. However, exposing your MCP server to the internet without proper security measures creates significant risks.

In this challenge, you will learn how to secure your MCP server by implementing API key authentication, enabling safe remote access while protecting your tools and resources from unauthorized use.

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

### Remote MCP Architecture
```
Client Application → HTTPS Request → API Gateway/Load Balancer → MCP Server
                   (with API Key)    (SSL Termination)      (Authenticated)
```

## Description

In this challenge, you will enhance your Weather MCP Server from Challenge 02 to support remote access with API key authentication. You'll deploy it as a web service and secure it properly.

### Task 1: Convert MCP Server to Web API

Transform your console-based MCP server into a web API that can handle HTTP requests:

1. **Create a new ASP.NET Core Web API project**:
```bash
dotnet new webapi -n SecureWeatherMcpServer
cd SecureWeatherMcpServer
```

2. **Add required NuGet packages**:
```bash
dotnet add package ModelContextProtocol
dotnet add package Microsoft.AspNetCore.Authentication
dotnet add package Microsoft.Extensions.Logging
dotnet add package Swashbuckle.AspNetCore
```

3. **Configure the web API** to expose MCP endpoints over HTTP instead of stdio.

### Task 2: Implement API Key Authentication

Create a secure API key authentication system:

1. **Create an API Key Authentication Handler**:
```csharp
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    
    // Implementation details
}
```

2. **API Key Storage**: Implement secure storage for API keys (in-memory for this challenge, but consider Azure Key Vault for production)

3. **Key Validation**: Validate incoming API keys against your stored keys

4. **Authentication Middleware**: Configure ASP.NET Core to use your API key authentication

### Task 3: Secure MCP Endpoints

Create secure HTTP endpoints for MCP operations:

1. **Tools Endpoint**: `GET /api/mcp/tools` - List available tools (requires authentication)
2. **Tool Execution Endpoint**: `POST /api/mcp/tools/{toolName}` - Execute a specific tool (requires authentication)
3. **Health Endpoint**: `GET /api/health` - Health check (public, no authentication required)

Example controller structure:
```csharp
[ApiController]
[Route("api/mcp")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class McpController : ControllerBase
{
    [HttpGet("tools")]
    public async Task<IActionResult> GetTools()
    {
        // Return available tools
    }
    
    [HttpPost("tools/{toolName}")]
    public async Task<IActionResult> ExecuteTool(string toolName, [FromBody] JsonElement arguments)
    {
        // Execute the specified tool with given arguments
    }
}
```

### Task 4: Add Security Headers and CORS

Implement additional security measures:

1. **Security Headers**: Add security headers to prevent common attacks
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});
```

2. **CORS Configuration**: Configure Cross-Origin Resource Sharing for web clients
3. **HTTPS Enforcement**: Ensure all communication is encrypted
4. **Request Validation**: Validate all input parameters

### Task 5: Create Admin Endpoints

Add administrative functionality for managing API keys:

1. **Generate API Key**: `POST /api/admin/keys` - Generate new API keys
2. **List API Keys**: `GET /api/admin/keys` - List all API keys (without showing the actual key)
3. **Revoke API Key**: `DELETE /api/admin/keys/{keyId}` - Revoke an API key

These endpoints should use a different authentication mechanism (e.g., admin token or Azure AD).

### Task 6: Update MCP Client

Modify your MCP client from Challenge 03 to work with the remote, secured server:

1. **HTTP Transport**: Replace stdio transport with HTTP transport
2. **API Key Configuration**: Add API key to all requests
3. **Error Handling**: Handle authentication errors gracefully
4. **Connection Management**: Implement proper HTTP connection management

Example client code:
```csharp
public class HttpMcpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public HttpMcpClient(string baseUrl, string apiKey)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _apiKey = apiKey;
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    }
    
    public async Task<IEnumerable<Tool>> GetToolsAsync()
    {
        var response = await _httpClient.GetAsync("api/mcp/tools");
        response.EnsureSuccessStatusCode();
        // Parse and return tools
    }
}
```

### Task 7: Testing and Validation

Test your secure MCP server thoroughly:

1. **Authentication Testing**: Verify that requests without API keys are rejected
2. **Authorization Testing**: Ensure only valid API keys can access protected endpoints
3. **Tool Functionality**: Confirm that weather tools still work correctly
4. **Performance Testing**: Test under load to ensure security doesn't impact performance
5. **Security Scanning**: Use tools like OWASP ZAP to scan for vulnerabilities

### Task 8: Deployment Considerations

Prepare for production deployment:

1. **Environment Configuration**: Use different API keys for development, staging, and production
2. **Logging**: Implement comprehensive security logging
3. **Monitoring**: Set up alerts for failed authentication attempts
4. **Backup Keys**: Plan for API key rotation and emergency access

## Success Criteria

- ✅ **Web API Conversion**: MCP server successfully converted to ASP.NET Core Web API
- ✅ **API Key Authentication**: Robust API key authentication system implemented
- ✅ **Secure Endpoints**: All MCP endpoints require valid authentication
- ✅ **Security Headers**: Appropriate security headers implemented
- ✅ **Admin Functionality**: API key management endpoints working
- ✅ **Updated Client**: MCP client successfully connects to remote secured server
- ✅ **Weather Tools**: Original weather functionality preserved and accessible
- ✅ **Error Handling**: Proper error responses for authentication failures
- ✅ **Documentation**: Clear API documentation with authentication requirements
- ✅ **Testing**: Comprehensive security and functionality testing completed

## Learning Resources

- [ASP.NET Core Web API Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Custom Authentication in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/custom)
- [API Security Best Practices](https://owasp.org/www-project-api-security/)
- [Azure Key Vault for Secrets Management](https://docs.microsoft.com/en-us/azure/key-vault/)
- [HTTP Security Headers](https://owasp.org/www-project-secure-headers/)
- [CORS in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [Model Context Protocol Security Guidelines](https://modelcontextprotocol.io/docs/security)
- [HTTPS Enforcement in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [Rate Limiting in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [API Versioning Best Practices](https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/versioning)