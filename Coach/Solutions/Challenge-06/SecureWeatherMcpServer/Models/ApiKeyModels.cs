namespace SecureWeatherMcpServer.Models;

public class ApiKeyInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Permissions { get; set; } = new();
}

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class CreateApiKeyResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class ApiKeyListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = new();
}
