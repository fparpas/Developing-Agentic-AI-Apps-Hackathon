using System.Text.Json;
using TravelMcpServer.Services;

namespace TravelMcpServer.Http;

/// <summary>
/// HTTP client wrapper for Amadeus API with built-in authentication
/// </summary>
public class AmadeusHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly AmadeusAuthService _authService;
    private readonly string _baseUrl;
    
    public AmadeusHttpClient(HttpClient httpClient, string clientId, string clientSecret, string baseUrl = "https://test.api.amadeus.com")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authService = new AmadeusAuthService(httpClient, clientId, clientSecret, baseUrl);
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTTP response message</returns>
    public async Task<HttpResponseMessage> GetAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var fullUrl = BuildFullUrl(endpoint);
        return await _authService.SendAuthorizedRequestAsync(HttpMethod.Get, fullUrl);
    }

    /// <summary>
    /// Sends a POST request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="content">Request content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTTP response message</returns>
    public async Task<HttpResponseMessage> PostAsync(string endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var fullUrl = BuildFullUrl(endpoint);
        return await _authService.SendAuthorizedRequestAsync(HttpMethod.Post, fullUrl, content);
    }

    /// <summary>
    /// Sends a PUT request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="content">Request content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTTP response message</returns>
    public async Task<HttpResponseMessage> PutAsync(string endpoint, HttpContent? content = null, CancellationToken cancellationToken = default)
    {
        var fullUrl = BuildFullUrl(endpoint);
        return await _authService.SendAuthorizedRequestAsync(HttpMethod.Put, fullUrl, content);
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTTP response message</returns>
    public async Task<HttpResponseMessage> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var fullUrl = BuildFullUrl(endpoint);
        return await _authService.SendAuthorizedRequestAsync(HttpMethod.Delete, fullUrl);
    }

    /// <summary>
    /// Sends a GET request and returns the response as a JsonDocument
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument containing the response</returns>
    public async Task<JsonDocument> GetJsonAsync(string endpoint, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var url = BuildUrlWithParameters(endpoint, parameters);
        return await GetJsonAsync(url, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request and returns the response as a JsonDocument
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument containing the response</returns>
    public async Task<JsonDocument> GetJsonAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(endpoint, cancellationToken);
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
        return JsonDocument.Parse(content);
    }

    /// <summary>
    /// Sends a POST request with JSON content and returns the response as a JsonDocument
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="data">Object to serialize as JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument containing the response</returns>
    public async Task<JsonDocument> PostJsonAsync(string endpoint, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await PostJsonAsync(endpoint, content, cancellationToken);
    }

    /// <summary>
    /// Sends a POST request and returns the response as a JsonDocument
    /// </summary>
    /// <param name="endpoint">API endpoint (relative to base URL)</param>
    /// <param name="requestContent">Request content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument containing the response</returns>
    public async Task<JsonDocument> PostJsonAsync(string endpoint, HttpContent? requestContent = null, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync(endpoint, requestContent, cancellationToken);
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);       
        
        return JsonDocument.Parse(content);
    }

    /// <summary>
    /// Gets the current access token
    /// </summary>
    /// <returns>Current access token</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        return await _authService.GetAccessTokenAsync();
    }

    /// <summary>
    /// Forces a refresh of the access token
    /// </summary>
    /// <returns>New access token</returns>
    public async Task<string> RefreshTokenAsync()
    {
        return await _authService.RefreshTokenAsync();
    }

    /// <summary>
    /// Gets information about the current token state
    /// </summary>
    /// <returns>Token information</returns>
    public TokenInfo GetTokenInfo()
    {
        return _authService.GetTokenInfo();
    }

    /// <summary>
    /// Creates a URL with query parameters
    /// </summary>
    /// <param name="endpoint">Base endpoint</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Full URL with query string</returns>
    public string BuildUrlWithParameters(string endpoint, Dictionary<string, string> parameters)
    {
        var fullUrl = BuildFullUrl(endpoint);
        
        if (parameters?.Any() == true)
        {
            var queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            fullUrl += "?" + queryString;
        }
        
        return fullUrl;
    }

    /// <summary>
    /// Builds the full URL from endpoint
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <returns>Full URL</returns>
    private string BuildFullUrl(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        // Handle both absolute and relative URLs
        if (endpoint.StartsWith("http://") || endpoint.StartsWith("https://"))
        {
            return endpoint;
        }

        // Ensure endpoint starts with /
        if (!endpoint.StartsWith("/"))
        {
            endpoint = "/" + endpoint;
        }

        return _baseUrl + endpoint;
    }

    public void Dispose()
    {
        _authService?.Dispose();
        // Note: We don't dispose _httpClient as it might be managed by DI container
    }
}

/// <summary>
/// Extension methods for AmadeusHttpClient
/// </summary>
public static class AmadeusHttpClientExtensions
{
    /// <summary>
    /// Extension method to read JSON document from AmadeusHttpClient (similar to existing HttpClientExt)
    /// </summary>
    /// <param name="client">AmadeusHttpClient instance</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument</returns>
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this AmadeusHttpClient client, string endpoint, CancellationToken cancellationToken = default)
    {
        return await client.GetJsonAsync(endpoint, cancellationToken);
    }

    /// <summary>
    /// Extension method to read JSON document with parameters
    /// </summary>
    /// <param name="client">AmadeusHttpClient instance</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="parameters">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JsonDocument</returns>
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this AmadeusHttpClient client, string endpoint, Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var url = client.BuildUrlWithParameters(endpoint, parameters);
        return await client.GetJsonAsync(url, cancellationToken);
    }
}