using Microsoft.Identity.Client;

/// <summary>
/// Service for handling Microsoft Entra ID authentication using MSAL
/// </summary>
public class EntraIdAuthService
{
    private readonly IConfidentialClientApplication _clientApp;
    private readonly string[] _scopes;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public EntraIdAuthService(string clientId, string clientSecret, string tenantId, string[] scopes)
    {
        if (string.IsNullOrEmpty(clientId))
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));
        if (string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException("Client Secret cannot be null or empty", nameof(clientSecret));
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));

        // If no scopes provided, use the client ID as the audience (api://clientId/.default)
        _scopes = scopes;

        var authority = $"https://login.microsoftonline.com/{tenantId}";
        
        _clientApp = ConfidentialClientApplicationBuilder   
            .Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(authority)
            .Build();
    }

    /// <summary>
    /// Gets an access token using client credentials flow
    /// </summary>
    /// <returns>Access token string</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        // Check if we have a cached token that's still valid (with 5-minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow.AddMinutes(5) < _tokenExpiry)
        {
            return _cachedToken;
        }

        try
        {
            var result = await _clientApp
                .AcquireTokenForClient(_scopes)
                .ExecuteAsync();

            _cachedToken = result.AccessToken;
            _tokenExpiry = result.ExpiresOn.UtcDateTime;

            return result.AccessToken;
        }
        catch (MsalException ex)
        {
            throw new InvalidOperationException($"Failed  to acquire token from Microsoft Entra ID: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets an authorization header value with Bearer token
    /// </summary>
    /// <returns>Authorization header value</returns>
    public async Task<string> GetAuthorizationHeaderAsync()
    {
        var token = await GetAccessTokenAsync();
        return $"Bearer {token}";
    }
}
