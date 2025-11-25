# Challenge 09 - C# - Secure Your Remote MCP Server with an API Key

[< Previous Challenge](./Challenge-08-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-10-csharp.md)

[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-09-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-09-python.md)

## Introduction

Previously, you worked with MCP servers and clients in local and remote environments without authentication or authorization. While that may suffice for local development or trusted scenarios, it is not suitable for production. When deploying remote MCP servers over HTTP/HTTPS, you must implement robust authentication for all clients. Exposing an unsecured MCP server to the internet creates significant security risks (abuse of tools, data exfiltration, quota exhaustion, malicious chaining, etc.).

In this challenge, you will secure the existing MCP Weather server by introducing API key authentication. This provides a simple, explicit credential mechanism and prepares the code structure so you can later upgrade to standards‑based authorization (OAuth 2.1 / OIDC, signed tokens, per‑principal policies) with minimal refactoring.

## Concepts

### API Key Authentication

API keys are a simple and effective method for authenticating clients to your MCP server. They provide:

- **Client identification**: Each client receives a unique key used to identify requests.
- **Access control**: Keys can be revoked or assigned distinct permission levels.
- **Usage tracking**: Monitor which clients initiate requests.
- **Rate limiting**: Control request frequency per client.

### MCP Security Considerations
When securing MCP servers, consider:

- **Transport security**: Use HTTPS for encrypted communication.
- **Authentication**: Verify client identity before processing requests.
- **Authorization**: Control which tools/resources clients can access.
- **Input validation**: Sanitize all inputs to prevent injection attacks.
- **Audit logging**: Track requests for security monitoring.
- **Rate limiting**: Prevent abuse and denial‑of‑service (DoS) attacks.


#### MCP Server Authorization (High‑Level Overview)

The Model Context Protocol includes an authorization model aligned with OAuth concepts for HTTP transports. This challenge deliberately starts simpler (static API key) so you can focus on the mechanics of securing endpoints. Your handler, routing, and middleware ordering should make it trivial to swap in a standards‑compliant token validator later. See the specs: [MCP Authorization Standards Compliance](https://modelcontextprotocol.io/specification/2025-06-18/basic/authorization#standards-compliance).

## Description

In this challenge, you will upgrade the existing Weather MCP server so it requires an API key for every MCP request and supports secure remote access over HTTP. The work includes:

1. Confirming the server is exposed via HTTP (remote‑capable) and not only local process transport.
2. Adding a custom authentication handler that validates an API key from a header.
3. Registering the authentication and authorization middleware in the correct ASP.NET Core order.
4. Requiring authorization for the MCP endpoint route.
5. Updating your MCP client to send the API key header.


> ℹ️ While API keys are a simple way to secure your server, it is generally more secure to authenticate clients using an identity provider such as Microsoft Entra ID (formerly Azure AD) with OAuth 2.0 or OpenID Connect flows. These modern methods provide stronger security, user and application identities, token expiration, and advanced access controls. For production scenarios, consider integrating with an identity provider instead of relying solely on API keys. Refer to the solution in the `Coach/` directory for an example of implementing OAuth 2.0 authentication with Entra ID.

### Task 1: Convert MCP Server to Remote MCP Server

Ensure that your MCP server runs remotely over HTTP and can handle incoming requests.

### Task 2: Implement API Key Authentication Handler

Add a new folder `Authentication` (if not already present) and create `ApiKeyAuthenticationHandler.cs` with the following (simplified) implementation:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

/// <summary>
/// Options for the API Key authentication scheme.
/// </summary>
/// <remarks>
/// This simple scheme extracts an API key from a configurable HTTP header (default: <c>X-API-Key</c>).
/// For production scenarios consider:
/// 1. Storing keys securely (Azure Key Vault, database with hashing, etc.).
/// 2. Supporting key rotation (multiple active keys with expirations).
/// 3. Adding rate limiting and anomaly detection per key.
/// 4. Moving to a stronger, token-based (OAuth 2.1 / OIDC) authorization model when user / app identity is required.
/// </remarks>
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>The canonical scheme name.</summary>
    public const string DefaultScheme = "ApiKey";

    /// <summary>The scheme name exposed to ASP.NET Core.</summary>
    public string Scheme => DefaultScheme;

    /// <summary>The name of the HTTP header from which to read the API key.</summary>
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";
}

/// <summary>
/// Authentication handler that validates a static API key supplied via a header.
/// </summary>
/// <remarks>
/// This implementation is intentionally minimal for challenge purposes:
/// - Uses a hard-coded key for demonstration.
/// - Treats the raw key value as the authenticated principal's name.
/// - Does not differentiate scopes/roles or persist usage metrics.
///
/// Hardcoded secrets MUST NOT be used in production. Replace <see cref="ValidateApiKey"/> with
/// a call to a secure key store or validation service. Consider hashing stored keys and comparing
/// in constant time to reduce timing attack risk. Avoid logging full keys; if logging is necessary, log only a short prefix.
/// </remarks>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// Attempts to authenticate the request by extracting and validating the API key.
    /// </summary>
    /// <returns>An <see cref="AuthenticateResult"/> indicating success or failure.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            return AuthenticateResult.Fail("API Key was not provided");
        }

        var apiKey = extractedApiKey.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.Fail("API Key was not provided");
        }

        var isValidKey = ValidateApiKey(apiKey);
        if (!isValidKey)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, apiKey),
        };

        var identity = new ClaimsIdentity(claims, Options.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Options.Scheme);

        return AuthenticateResult.Success(ticket);
    }

    /// <summary>
    /// Validates the provided API key.
    /// </summary>
    /// <param name="key">The raw API key string supplied by the client.</param>
    /// <returns><c>true</c> if the key is valid; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Replace this hard-coded comparison with a secure lookup (e.g., hashed comparison from a store).
    /// Use constant-time comparison to minimize timing attack vectors when keys are user-generated.
    /// </remarks>
    private bool ValidateApiKey(string key)
    {
        return string.Compare(key, "<Add your API Key>", StringComparison.Ordinal) == 0;
    }

    /// <summary>
    /// Handles the authentication challenge (401) by returning a WWW-Authenticate header and message.
    /// </summary>
    /// <param name="properties">Authentication properties.</param>
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers["WWW-Authenticate"] = $"{Options.Scheme} realm=\"API\"";
        await Response.WriteAsync("Unauthorized: Valid API key required");
    }

    /// <summary>
    /// Handles forbidden (403) responses when authentication succeeded but authorization failed.
    /// </summary>
    /// <param name="properties">Authentication properties.</param>
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        await Response.WriteAsync("Forbidden: Insufficient permissions");
    }
}

```

### Task 3: Register Authentication & Protect MCP Endpoints

In your `Program.cs` configure the authentication scheme and protect your MCP endpoints.

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "ApiKey";
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
})
.AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options =>
{
    options.ApiKeyHeaderName = "X-API-Key";
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "MCP server is running!");

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization();

app.Run();
```

### Task 4: Update the MCP Client to Send the API Key

Modify the MCP client from a previous challenge to include the API key when calling the secured remote server.


```csharp
  var clientTransport = new SseClientTransport(new SseClientTransportOptions()
                {
                    Endpoint = new Uri(remoteMCP),
                    AdditionalHeaders = new Dictionary<string, string>
                    {
                        { "X-API-Key", "<Your API Key>" }
                    }
                });
```

### Task 5: Verify Secure Communication Between MCP Client and Server

Ensure your MCP client includes the API key in request headers when communicating with the secured remote MCP server. Test the connection by sending requests:

- If the API key is missing or incorrect, the server should respond with HTTP 401 (Unauthorized).
- If the API key is valid, the client should receive successful responses from protected MCP endpoints.

Verify that only requests with the correct API key are processed. This confirms your authentication mechanism works as intended and demonstrates secure communication between the MCP client and the remote server.

## Success Criteria

- ✅ Requests without an API key are rejected (authentication enforced).
- ✅ Only valid API keys can access protected endpoints (authorization verified).
- ✅ API key authentication system is implemented and functional.
- ✅ All MCP endpoints require valid authentication.
- ✅ MCP client successfully connects to the secured remote server.

## Learning Resources

- [ASP.NET Core Web API Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [API Security Best Practices](https://owasp.org/www-project-api-security/)
- [Azure Key Vault for Secrets Management](https://docs.microsoft.com/en-us/azure/key-vault/)
- [HTTP Security Headers](https://owasp.org/www-project-secure-headers/)
- [CORS in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [Model Context Protocol Security Guidelines](https://modelcontextprotocol.io/docs/security)
- [HTTPS Enforcement in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [Rate Limiting in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/rate-limit)
 