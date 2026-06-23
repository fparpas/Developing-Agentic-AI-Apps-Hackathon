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
/// Hard-coded secrets MUST NOT be used in production. Replace <see cref="ValidateApiKey"/> with
/// a call to a secure key store or validation service. Consider hashing stored keys and comparing
/// constant-time to prevent timing attacks. Avoid logging full keys; if logging is necessary, log only a prefix.
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
