using System.Text;
using System.Text.Json;

namespace TravelMcpServer.Services;

/// <summary>
/// Authentication service for Amadeus API using OAuth2 client credentials flow
/// </summary>
public class AmadeusAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _baseUrl;
    
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    public AmadeusAuthService(HttpClient httpClient, string clientId, string clientSecret, string baseUrl = "https://test.api.amadeus.com")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Gets a valid access token, refreshing if necessary
    /// </summary>
    /// <returns>Valid access token</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        await _tokenSemaphore.WaitAsync();
        try
        {
            // Check if current token is still valid (with 5-minute buffer)
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            {
                return _accessToken;
            }

            // Get new token
            _accessToken = await RequestNewTokenAsync();
            return _accessToken;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <summary>
    /// Forces a refresh of the access token
    /// </summary>
    /// <returns>New access token</returns>
    public async Task<string> RefreshTokenAsync()
    {
        await _tokenSemaphore.WaitAsync();
        try
        {
            _accessToken = await RequestNewTokenAsync();
            return _accessToken;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <summary>
    /// Creates an HTTP request message with proper authorization header
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="requestUri">Request URI</param>
    /// <returns>HttpRequestMessage with authorization header</returns>
    public async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string requestUri)
    {
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    /// <summary>
    /// Sends an authorized HTTP request
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="requestUri">Request URI</param>
    /// <param name="content">Optional request content</param>
    /// <returns>HTTP response message</returns>
    public async Task<HttpResponseMessage> SendAuthorizedRequestAsync(HttpMethod method, string requestUri, HttpContent? content = null)
    {
        var request = await CreateAuthorizedRequestAsync(method, requestUri);
        if (content != null)
        {
            request.Content = content;
        }

        var response = await _httpClient.SendAsync(request);
        
        // If unauthorized, try refreshing token once
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await RefreshTokenAsync();
            
            // Retry with new token
            request = await CreateAuthorizedRequestAsync(method, requestUri);
            if (content != null)
            {
                request.Content = content;
            }
            response = await _httpClient.SendAsync(request);
        }

        return response;
    }

    /// <summary>
    /// Requests a new access token from Amadeus OAuth2 endpoint
    /// </summary>
    /// <returns>Access token</returns>
    private async Task<string> RequestNewTokenAsync()
    {
        var tokenUrl = $"{_baseUrl}/v1/security/oauth2/token";
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", _clientId),
            new("client_secret", _clientSecret)
        };

        using var content = new FormUrlEncodedContent(formData);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(tokenUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to obtain access token. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonResponse);
        var root = jsonDocument.RootElement;

        if (!root.TryGetProperty("access_token", out var tokenElement))
        {
            throw new InvalidOperationException("Access token not found in response");
        }

        var accessToken = tokenElement.GetString();
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new InvalidOperationException("Access token is null or empty");
        }

        // Parse expiry time
        if (root.TryGetProperty("expires_in", out var expiresElement) && expiresElement.TryGetInt32(out var expiresIn))
        {
            _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);
        }
        else
        {
            // Default to 30 minutes if expires_in is not provided
            _tokenExpiry = DateTime.UtcNow.AddMinutes(30);
        }

        return accessToken;
    }

    /// <summary>
    /// Gets token information for debugging/monitoring
    /// </summary>
    /// <returns>Token information</returns>
    public TokenInfo GetTokenInfo()
    {
        return new TokenInfo
        {
            HasToken = !string.IsNullOrEmpty(_accessToken),
            TokenExpiry = _tokenExpiry,
            IsExpired = DateTime.UtcNow >= _tokenExpiry,
            TimeUntilExpiry = _tokenExpiry > DateTime.UtcNow ? _tokenExpiry - DateTime.UtcNow : TimeSpan.Zero
        };
    }

    public void Dispose()
    {
        _tokenSemaphore?.Dispose();
    }
}

/// <summary>
/// Information about the current token state
/// </summary>
public record TokenInfo
{
    public bool HasToken { get; init; }
    public DateTime TokenExpiry { get; init; }
    public bool IsExpired { get; init; }
    public TimeSpan TimeUntilExpiry { get; init; }
}