using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.AspNetCore.Authentication;
using SecureWeatherMcpServer.Authentication;

var builder = WebApplication.CreateBuilder(args);


string entraTenantId = "<Your-Tenant-ID>";
string entraClientId = "<Your-Entra-Client-ID>";

// Configure to use HTTP only
builder.WebHost.UseUrls("http://localhost:5001");

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
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configure to validate tokens from our in-memory OAuth server
    options.Authority = $"https://login.microsoftonline.com/{entraTenantId}";
    options.Audience = entraClientId;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        // Add valid audiences - this is crucial for service principal tokens
        ValidAudiences = new[] { entraClientId, $"api://{entraClientId}" },
        // Add valid issuers
        ValidIssuers = new[]
        {
            $"https://sts.windows.net/{entraTenantId}/"
        },
        // Allow some flexibility in audience validation for service principals
        RequireAudience = true,
        // Ensure we validate the signing key
        RequireSignedTokens = true
        // NameClaimType = "name",
        // RoleClaimType = "roles"
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var name = context.Principal?.Identity?.Name ?? "unknown";
            var email = context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
            Console.WriteLine($"Token validated for: {name} ({email})");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"Challenging client to authenticate with Entra ID");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("📨 JWT Token received from client");
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseRouting();

// app.MapGet("/", () => "MCP server is running!");

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization();

app.Run();