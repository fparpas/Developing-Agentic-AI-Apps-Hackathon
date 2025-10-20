using System.ClientModel;
using System.Security.Principal;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TravelMultiAgentClient.Agents;
using TravelMultiAgentClient.Services;

namespace TravelMultiAgentClient;

class Program
{
    private static HostApplicationBuilder? builder;
    
    static async Task Main(string[] args)
    {
        // Build configuration
        var configurationService = new ConfigurationService();

        // Extract configuration values first
        var endpoint = configurationService.GetRequiredValue("AzureOpenAI:Endpoint");
        var deploymentName = configurationService.GetValue("AzureOpenAI:ModelId", "gpt-4o");
        var apiKey = configurationService.GetRequiredValue("AzureOpenAI:ApiKey");
        var otlpEndpoint = configurationService.GetRequiredValue("Observability:AspireUrl");
        var foundryAgentId = configurationService.GetRequiredValue("FoundryAgentService:AgentId");
        var foundryEndpoint = configurationService.GetRequiredValue("FoundryAgentService:Endpoint");
        var appInsightsConnectionString = configurationService.GetRequiredValue("Observability:AppInsightsConnectionString");
        
        // Setup tracing with telemetry service
        var telemetryService = new TelemetryService();
        using var tracerProvider = telemetryService.CreateTracerProvider(otlpEndpoint, appInsightsConnectionString);

        // Build host
        builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<IConfiguration>(configurationService.Configuration);
        builder.Services.AddSingleton<McpClientService>();
  
        // Setup structured logging with OpenTelemetry
        // var serviceCollection = new ServiceCollection();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Debug)
            .AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TelemetryService.GetServiceName(), serviceVersion: "1.0.0"));
                options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(otlpEndpoint));                
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
            }));

        // Add a chat client to the service collection.
        builder.Services.AddSingleton<IChatClient>(new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient());

        // Add a persistent agents client to the service collection.
        // For local development, use AzureCliCredential (requires 'az login')
        builder.Services.AddSingleton(new PersistentAgentsClient(
            foundryEndpoint,
            new DefaultAzureCredential()));

        var chatClient = builder.Services.BuildServiceProvider().GetRequiredService<IChatClient>();
        var persistentAgentsClient = builder.Services.BuildServiceProvider().GetRequiredService<PersistentAgentsClient>();
        var mcpClientService = builder.Services.BuildServiceProvider().GetRequiredService<McpClientService>();

        AIAgent[] agentsAsTools = new AIAgent[]
               {
            new TravelPolicyAgent(persistentAgentsClient, foundryAgentId).Agent,
            new FlightAgent(chatClient,mcpClientService).Agent,
            new HotelAgent(chatClient,mcpClientService).Agent,
            new ActivityAgent(chatClient,mcpClientService).Agent,
            new TransferAgent(chatClient,mcpClientService).Agent,
            new ReferenceAgent(chatClient,mcpClientService).Agent
               };

        var mainOrchestratorAgent = new MainOrchestratorAgent(
            builder.Services.BuildServiceProvider().GetRequiredService<IChatClient>(),
            agentsAsTools);
        builder.Services.AddSingleton(mainOrchestratorAgent);

        var serviceProvider = builder.Build();

        var logger = serviceProvider.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🌟 Welcome to Travel Multi-Agent Assistant! 🌟");

            var orchestratorAgent = serviceProvider.Services.GetRequiredService<MainOrchestratorAgent>();

            // await StartInteractiveChat(flightAgent.Agent);
            logger.LogInformation("✅ All agents initialized successfully");


            logger.LogInformation("🤖 Travel Agent Assistant created");
            // Start interactive session
            await StartInteractiveChat(orchestratorAgent.Agent);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while running the application");
        }
    }
   
    private static async Task StartInteractiveChat(AIAgent aIAgent)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        AgentThread agentThread = aIAgent.GetNewThread();
        var messages = new List<ChatMessage>();
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();

            var userInput = Console.ReadLine();
            messages.Add(new(ChatRole.User, userInput));

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                agentThread = aIAgent.GetNewThread();
                
                // Run agent with user input and agent thread
                var response = await aIAgent.RunAsync(messages, agentThread);
                messages.AddRange(response.Messages);
                
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
            }
        }
    }
    
}