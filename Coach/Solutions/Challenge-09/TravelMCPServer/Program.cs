using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using System.Net.Http.Headers;
using TravelMcpServer.Http;
using TravelMcpServer.Services;



var httpPort = int.Parse(Environment.GetEnvironmentVariable("HttpPort") ?? "8080");
    var httpHost = Environment.GetEnvironmentVariable("HttpHost") ?? "localhost";
    
    Console.WriteLine($"🌐 Starting Travel MCP Server in HTTP mode on http://{httpHost}:{httpPort}");
    
    // Create WebApplication for HTTP mode
    var builder = WebApplication.CreateBuilder(args);
    
    // Add configuration
    builder.Configuration.AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
    
    // Add logging
    builder.Services.AddLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    });

// Configure MCP server with HTTP transport
builder.Services.AddMcpServer()
        .WithHttpTransport()
        .WithToolsFromAssembly();
    
    // Configure services
    ConfigureServices(builder.Services, builder.Configuration);
    
    var app = builder.Build();
    
    // Configure HTTP pipeline
    app.UseRouting();
    app.MapMcp();
    
    // Display startup information
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 Travel MCP Server starting in HTTP mode...");
    logger.LogInformation($"🌐 HTTP Server: http://{httpHost}:{httpPort}");

    app.Run($"http://{httpHost}:{httpPort}");


static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Configure HTTP client for travel-related API calls
    services.AddSingleton<HttpClient>(_ =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("travel-mcp-server", "1.0"));
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    });

    // Register Amadeus authentication service
    services.AddSingleton<AmadeusAuthService>(serviceProvider =>
    {
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var httpClient = serviceProvider.GetRequiredService<HttpClient>();

        var clientId = config["Amadeus:ClientId"] ?? throw new InvalidOperationException("Amadeus:ClientId is required");
        var clientSecret = config["Amadeus:ClientSecret"] ?? throw new InvalidOperationException("Amadeus:ClientSecret is required");
        var baseUrl = config["Amadeus:BaseUrl"] ?? "https://test.api.amadeus.com";

        return new AmadeusAuthService(httpClient, clientId, clientSecret, baseUrl);
    });

    // Register Amadeus HTTP client wrapper
    services.AddSingleton<AmadeusHttpClient>(serviceProvider =>
    {
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var httpClient = serviceProvider.GetRequiredService<HttpClient>();

        var clientId = config["Amadeus:ClientId"] ?? throw new InvalidOperationException("Amadeus:ClientId is required");
        var clientSecret = config["Amadeus:ClientSecret"] ?? throw new InvalidOperationException("Amadeus:ClientSecret is required");
        var baseUrl = config["Amadeus:BaseUrl"] ?? "https://test.api.amadeus.com";

        return new AmadeusHttpClient(httpClient, clientId, clientSecret, baseUrl);
    });
}