using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ModelContextProtocol;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add ASP.NET Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure for HTTP transport instead of stdio for remote access
builder.Services.AddMcpServer()
    .WithHttpTransport() // Use HTTP transport for remote access
    .WithToolsFromAssembly();

// Configure HttpClient for weather.gov API
builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});


var app = builder.Build();

app.MapMcp();

app.Run();

Console.WriteLine("🚀 Weather MCP Server starting with HTTP transport...");
Console.WriteLine("🌐 Server is now accessible remotely via HTTP");
Console.WriteLine("📡 Clients can connect to HTTP endpoints");
Console.WriteLine("⏹️  Press Ctrl+C to stop the server");

