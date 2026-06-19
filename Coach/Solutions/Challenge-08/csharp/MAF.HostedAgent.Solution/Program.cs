using Azure.AI.AgentServer.Core;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

// Challenge 08 Solution - Host the Challenge 6 weather + remote MCP agent as a Foundry Hosted Agent.
//
// FOUNDRY_PROJECT_ENDPOINT and AZURE_AI_MODEL_DEPLOYMENT_NAME are injected automatically when the
// agent runs inside Foundry Agent Service. For local testing, set them as environment variables or
// fill them in (plus WEATHER_MCP_ENDPOINT) in appsettings.json / appsettings.Development.json.
// Environment variables take precedence over the appsettings files.

var builder = AgentHost.CreateBuilder(args);

// Load appsettings from the app's base directory so the files

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

// Configuration merges appsettings.json, appsettings.{Environment}.json, and environment variables
// (env vars take precedence, so Foundry's runtime-injected values override the files).
var projectEndpoint = new Uri(configuration["FOUNDRY_PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set."));
var deployment = configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"] ?? "gpt-4o";
var mcpServerUrl = configuration["WEATHER_MCP_ENDPOINT"]
    ?? throw new InvalidOperationException("WEATHER_MCP_ENDPOINT is not set.");

// 1. Connect to the remote Weather MCP server from Challenge 6 and discover its tools.
var mcpClient = await McpClient.CreateAsync(
    new HttpClientTransport(new HttpClientTransportOptions { Endpoint = new Uri(mcpServerUrl) }));

var mcpTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"Found {mcpTools.Count} MCP tools from {mcpServerUrl}");

// 2. Build the weather agent on top of the Foundry model deployment and register the MCP tools.
AIAgent agent = new AIProjectClient(projectEndpoint, new DefaultAzureCredential())
    .AsAIAgent(
        model: deployment,
        instructions: "You are a helpful assistant that answers weather questions using the available MCP tools.",
        name: "weather-hosted-agent",
        tools: [.. mcpTools.Cast<AITool>()]);

// 3. Host the agent behind the Foundry Responses protocol (OpenAI-compatible /responses endpoint).
builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol("responses", endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();
