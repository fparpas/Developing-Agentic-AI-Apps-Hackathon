using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using SecureWeatherMcpServer.Services;

namespace SecureWeatherMcpServer.Authentication;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public string Scheme => DefaultScheme;
    public string ApiKeyHeaderName { get; set; } = "X-API-Key";
}

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
    }

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

        var isValidKey = await _apiKeyService.ValidateApiKeyAsync(apiKey);
        if (!isValidKey)
        {
            return AuthenticateResult.Fail("Invalid API Key");
        }

        var keyInfo = await _apiKeyService.GetApiKeyInfoAsync(apiKey);
        if (keyInfo == null)
        {
            return AuthenticateResult.Fail("API Key not found");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, keyInfo.Name),
            new Claim("ApiKeyId", keyInfo.Id),
            new Claim("ApiKeyName", keyInfo.Name)
        };

        var identity = new ClaimsIdentity(claims, Options.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Options.Scheme);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers["WWW-Authenticate"] = $"{Options.Scheme} realm=\"API\"";
        await Response.WriteAsync("Unauthorized: Valid API key required");
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        await Response.WriteAsync("Forbidden: Insufficient permissions");
    }
}
