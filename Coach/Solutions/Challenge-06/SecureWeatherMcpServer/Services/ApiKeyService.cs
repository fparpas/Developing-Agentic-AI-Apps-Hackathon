using SecureWeatherMcpServer.Models;

namespace SecureWeatherMcpServer.Services;

public interface IApiKeyService
{
    Task<bool> ValidateApiKeyAsync(string apiKey);
    Task<ApiKeyInfo?> GetApiKeyInfoAsync(string apiKey);
    Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<List<ApiKeyListItem>> GetApiKeysAsync();
    Task<bool> RevokeApiKeyAsync(string id);
    Task UpdateLastUsedAsync(string apiKey);
}

public class ApiKeyService : IApiKeyService
{
    private readonly ILogger<ApiKeyService> _logger;
    private static readonly Dictionary<string, ApiKeyInfo> _apiKeys = new();
    private static readonly Dictionary<string, string> _keyToHashMap = new();

    public ApiKeyService(ILogger<ApiKeyService> logger)
    {
        _logger = logger;
        
        // Initialize with a default API key for demo purposes
        InitializeDefaultApiKey();
    }

    private void InitializeDefaultApiKey()
    {
        var defaultKey = "sk-demo-weather-api-key-12345";
        var defaultKeyHash = ComputeHash(defaultKey);
        var defaultApiKeyInfo = new ApiKeyInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Demo API Key",
            KeyHash = defaultKeyHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Permissions = new List<string> { "weather:read", "tools:list" }
        };

        _apiKeys[defaultKeyHash] = defaultApiKeyInfo;
        _keyToHashMap[defaultKey] = defaultKeyHash;
        
        _logger.LogInformation("Initialized default API key: {KeyId}", defaultApiKeyInfo.Id);
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        await Task.CompletedTask; // For async interface consistency
        
        if (string.IsNullOrEmpty(apiKey))
            return false;

        if (!_keyToHashMap.TryGetValue(apiKey, out var keyHash))
            return false;

        if (!_apiKeys.TryGetValue(keyHash, out var keyInfo))
            return false;

        return keyInfo.IsActive;
    }

    public async Task<ApiKeyInfo?> GetApiKeyInfoAsync(string apiKey)
    {
        await Task.CompletedTask; // For async interface consistency
        
        if (string.IsNullOrEmpty(apiKey))
            return null;

        if (!_keyToHashMap.TryGetValue(apiKey, out var keyHash))
            return null;

        return _apiKeys.TryGetValue(keyHash, out var keyInfo) ? keyInfo : null;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        await Task.CompletedTask; // For async interface consistency
        
        var apiKey = GenerateApiKey();
        var keyHash = ComputeHash(apiKey);
        var id = Guid.NewGuid().ToString();

        var apiKeyInfo = new ApiKeyInfo
        {
            Id = id,
            Name = request.Name,
            KeyHash = keyHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Permissions = request.Permissions ?? new List<string>()
        };

        _apiKeys[keyHash] = apiKeyInfo;
        _keyToHashMap[apiKey] = keyHash;

        _logger.LogInformation("Created new API key: {KeyId} for {KeyName}", id, request.Name);

        return new CreateApiKeyResponse
        {
            Id = id,
            Name = request.Name,
            ApiKey = apiKey,
            CreatedAt = apiKeyInfo.CreatedAt,
            Permissions = apiKeyInfo.Permissions
        };
    }

    public async Task<List<ApiKeyListItem>> GetApiKeysAsync()
    {
        await Task.CompletedTask; // For async interface consistency
        
        return _apiKeys.Values.Select(k => new ApiKeyListItem
        {
            Id = k.Id,
            Name = k.Name,
            CreatedAt = k.CreatedAt,
            LastUsedAt = k.LastUsedAt,
            IsActive = k.IsActive,
            Permissions = k.Permissions
        }).ToList();
    }

    public async Task<bool> RevokeApiKeyAsync(string id)
    {
        await Task.CompletedTask; // For async interface consistency
        
        var keyInfo = _apiKeys.Values.FirstOrDefault(k => k.Id == id);
        if (keyInfo != null)
        {
            keyInfo.IsActive = false;
            _logger.LogInformation("Revoked API key: {KeyId}", id);
            return true;
        }

        return false;
    }

    public async Task UpdateLastUsedAsync(string apiKey)
    {
        await Task.CompletedTask; // For async interface consistency
        
        if (_keyToHashMap.TryGetValue(apiKey, out var keyHash) &&
            _apiKeys.TryGetValue(keyHash, out var keyInfo))
        {
            keyInfo.LastUsedAt = DateTime.UtcNow;
        }
    }

    private static string GenerateApiKey()
    {
        var prefix = "sk-";
        var random = new Random();
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var key = new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        return prefix + key;
    }

    private static string ComputeHash(string input)
    {
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input)));
    }
}
