using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureWeatherMcpServer.Services;
using ModelContextProtocol;

namespace SecureWeatherMcpServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class McpController : ControllerBase
{
    private readonly ILogger<McpController> _logger;
    private readonly IWeatherService _weatherService;
    private readonly IApiKeyService _apiKeyService;

    public McpController(
        ILogger<McpController> logger,
        IWeatherService weatherService,
        IApiKeyService apiKeyService)
    {
        _logger = logger;
        _weatherService = weatherService;
        _apiKeyService = apiKeyService;
    }

    [HttpPost("tools/list")]
    public async Task<IActionResult> ListTools()
    {
        _logger.LogInformation("MCP tools list requested");

        var tools = new
        {
            tools = new object[]
            {
                new
                {
                    name = "get_current_weather",
                    description = "Get the current weather for a specified location",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new
                            {
                                type = "string",
                                description = "The city and country, e.g. 'London, UK'"
                            },
                            unit = new
                            {
                                type = "string",
                                description = "Temperature unit (celsius or fahrenheit)",
                                @enum = new[] { "celsius", "fahrenheit" }
                            }
                        },
                        required = new[] { "location" }
                    }
                },
                new
                {
                    name = "get_weather_forecast",
                    description = "Get weather forecast for a specified location",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            location = new
                            {
                                type = "string",
                                description = "The city and country, e.g. 'London, UK'"
                            },
                            days = new
                            {
                                type = "integer",
                                description = "Number of forecast days (1-7)",
                                minimum = 1,
                                maximum = 7
                            }
                        },
                        required = new[] { "location" }
                    }
                }
            }
        };

        await UpdateApiKeyLastUsed();
        return Ok(tools);
    }

    [HttpPost("tools/call")]
    public async Task<IActionResult> CallTool([FromBody] McpToolCallRequest request)
    {
        _logger.LogInformation("MCP tool call: {ToolName}", request.Name);

        try
        {
            object result = request.Name switch
            {
                "get_current_weather" => await HandleGetCurrentWeather(request.Arguments),
                "get_weather_forecast" => await HandleGetWeatherForecast(request.Arguments),
                _ => throw new ArgumentException($"Unknown tool: {request.Name}")
            };

            await UpdateApiKeyLastUsed();

            return Ok(new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        })
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", request.Name);
            return BadRequest(new { error = ex.Message });
        }
    }

    private async Task<object> HandleGetCurrentWeather(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.TryGetValue("location", out var locationObj))
            throw new ArgumentException("Location is required");

        var location = locationObj.ToString() ?? throw new ArgumentException("Invalid location");
        var unit = arguments.TryGetValue("unit", out var unitObj) ? unitObj.ToString() : "celsius";

        return await _weatherService.GetCurrentWeatherAsync(location, unit);
    }

    private async Task<object> HandleGetWeatherForecast(Dictionary<string, object>? arguments)
    {
        if (arguments == null || !arguments.TryGetValue("location", out var locationObj))
            throw new ArgumentException("Location is required");

        var location = locationObj.ToString() ?? throw new ArgumentException("Invalid location");
        var days = 5;

        if (arguments.TryGetValue("days", out var daysObj))
        {
            if (int.TryParse(daysObj.ToString(), out var parsedDays))
                days = Math.Clamp(parsedDays, 1, 7);
        }

        return await _weatherService.GetWeatherForecastAsync(location, days);
    }

    private async Task UpdateApiKeyLastUsed()
    {
        var apiKey = Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
        {
            await _apiKeyService.UpdateLastUsedAsync(apiKey);
        }
    }
}

public class McpToolCallRequest
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}
