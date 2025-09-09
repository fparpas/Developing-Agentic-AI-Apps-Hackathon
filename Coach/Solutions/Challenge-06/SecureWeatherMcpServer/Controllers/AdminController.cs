using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureWeatherMcpServer.Models;
using SecureWeatherMcpServer.Services;

namespace SecureWeatherMcpServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IApiKeyService _apiKeyService;

    public AdminController(ILogger<AdminController> logger, IApiKeyService apiKeyService)
    {
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    [HttpGet("apikeys")]
    public async Task<IActionResult> GetApiKeys()
    {
        _logger.LogInformation("Getting API keys list");
        var apiKeys = await _apiKeyService.GetApiKeysAsync();
        return Ok(new { apiKeys });
    }

    [HttpPost("apikeys")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        _logger.LogInformation("Creating new API key: {KeyName}", request.Name);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "API key name is required" });
        }

        var result = await _apiKeyService.CreateApiKeyAsync(request);
        return Ok(result);
    }

    [HttpDelete("apikeys/{id}")]
    public async Task<IActionResult> RevokeApiKey(string id)
    {
        _logger.LogInformation("Revoking API key: {KeyId}", id);

        var success = await _apiKeyService.RevokeApiKeyAsync(id);
        if (success)
        {
            return Ok(new { message = "API key revoked successfully" });
        }

        return NotFound(new { error = "API key not found" });
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
