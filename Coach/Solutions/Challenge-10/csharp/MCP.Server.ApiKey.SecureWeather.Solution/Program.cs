using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Configure to use HTTP only
builder.WebHost.UseUrls("http://localhost:5000");

// Add ASP.NET Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMcpServer()
    .WithHttpTransport() // Use HTTP transport for remote access
    .WithToolsFromAssembly()
    .WithHttpTransport();

// Configure HttpClientFactory for weather.gov API
builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "ApiKey";
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
})
.AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options =>
{
    options.ApiKeyHeaderName = "X-API-KEY";
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "MCP server is running!");

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization();

app.Run();