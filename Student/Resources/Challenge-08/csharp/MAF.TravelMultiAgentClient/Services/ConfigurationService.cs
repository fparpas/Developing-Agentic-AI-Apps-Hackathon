using Microsoft.Extensions.Configuration;

namespace TravelMultiAgentClient.Services;

public class ConfigurationService
{
    public IConfiguration Configuration { get; }

    public ConfigurationService()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<ConfigurationService>()
            .AddEnvironmentVariables()
            .Build();
    }

    public string GetRequiredValue(string key)
    {
        return Configuration[key] 
            ?? throw new InvalidOperationException($"{key} is required in configuration");
    }

    public string GetValue(string key, string defaultValue = "")
    {
        return Configuration[key] ?? defaultValue;
    }
}
