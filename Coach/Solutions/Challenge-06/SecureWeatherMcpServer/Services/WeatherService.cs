using ModelContextProtocol;
using SecureWeatherMcpServer.Services;

namespace SecureWeatherMcpServer.Services;

public interface IWeatherService
{
    Task<object> GetCurrentWeatherAsync(string location, string? unit = null);
    Task<object> GetWeatherForecastAsync(string location, int days = 5);
}

public class WeatherService : IWeatherService
{
    private readonly ILogger<WeatherService> _logger;
    private readonly HttpClient _httpClient;

    public WeatherService(ILogger<WeatherService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<object> GetCurrentWeatherAsync(string location, string? unit = null)
    {
        _logger.LogInformation("Getting current weather for {Location}", location);

        // Simulate weather API call - in a real implementation, you'd call an actual weather service
        await Task.Delay(100); // Simulate API delay

        var temperature = unit?.ToLower() == "fahrenheit" ? 
            Random.Shared.Next(30, 100) : 
            Random.Shared.Next(-10, 40);

        var conditions = new[] { "sunny", "cloudy", "rainy", "snowy", "partly cloudy" };
        var condition = conditions[Random.Shared.Next(conditions.Length)];

        return new
        {
            location = location,
            temperature = temperature,
            unit = unit?.ToLower() == "fahrenheit" ? "°F" : "°C",
            condition = condition,
            humidity = Random.Shared.Next(30, 90),
            windSpeed = Random.Shared.Next(0, 30),
            timestamp = DateTime.UtcNow
        };
    }

    public async Task<object> GetWeatherForecastAsync(string location, int days = 5)
    {
        _logger.LogInformation("Getting {Days}-day weather forecast for {Location}", days, location);

        // Simulate weather API call
        await Task.Delay(150);

        var forecast = new List<object>();
        var conditions = new[] { "sunny", "cloudy", "rainy", "snowy", "partly cloudy" };

        for (int i = 0; i < Math.Min(days, 7); i++)
        {
            var date = DateTime.UtcNow.AddDays(i);
            forecast.Add(new
            {
                date = date.ToString("yyyy-MM-dd"),
                highTemp = Random.Shared.Next(15, 35),
                lowTemp = Random.Shared.Next(-5, 15),
                condition = conditions[Random.Shared.Next(conditions.Length)],
                chanceOfRain = Random.Shared.Next(0, 100)
            });
        }

        return new
        {
            location = location,
            forecast = forecast,
            timestamp = DateTime.UtcNow
        };
    }
}
