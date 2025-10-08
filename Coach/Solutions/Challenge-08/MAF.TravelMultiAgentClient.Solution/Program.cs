using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using TravelMultiAgentClient.Agents;
using TravelMultiAgentClient.Services;

namespace TravelMultiAgentClient;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        // Extract configuration values first
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is required");
        var deploymentName = configuration["AzureOpenAI:ModelId"] ?? "gpt-4o";
        var apiKey = configuration["AzureOpenAI:ApiKey"];

        // Build host
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<IConfiguration>(configuration);
        builder.Services.AddSingleton<McpClientService>();
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        // Add a chat client to the service collection.
        builder.Services.AddSingleton<IChatClient>(new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient());

        builder.Services.AddSingleton<FlightAgent>();
        builder.Services.AddSingleton<HotelAgent>();
        builder.Services.AddSingleton<ActivityAgent>();
        builder.Services.AddSingleton<TransferAgent>();
        builder.Services.AddSingleton<ReferenceAgent>();
        builder.Services.AddSingleton<TravelCoordinatorAgent>();

        var serviceProvider = builder.Build();

        var logger = serviceProvider.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("🌟 Welcome to Travel Multi-Agent Assistant! 🌟");

            // Create specialized agents
            var flightAgent = serviceProvider.Services.GetRequiredService<FlightAgent>();
            var hotelAgent = serviceProvider.Services.GetRequiredService<HotelAgent>();
            var activityAgent = serviceProvider.Services.GetRequiredService<ActivityAgent>();
            var transferAgent = serviceProvider.Services.GetRequiredService<TransferAgent>();
            var referenceAgent = serviceProvider.Services.GetRequiredService<ReferenceAgent>();
            var coordinatorAgent = serviceProvider.Services.GetRequiredService<TravelCoordinatorAgent>();

            logger.LogInformation("✅ All agents initialized successfully");

            // // Create a sequential workflow with the TravelCoordinatorAgent as the lead agent
            // var workflow = AgentWorkflowBuilder.BuildSequential(new[]   
            // {
            //     coordinatorAgent.Agent,
            //     flightAgent.Agent,
            //     hotelAgent.Agent,
            //     activityAgent.Agent,
            //     transferAgent.Agent,
            //     referenceAgent.Agent
            //  });

            // // Create a concurrent workflow with all agents
            // var workflow = AgentWorkflowBuilder.BuildConcurrent(new[]   
            // {
            //     coordinatorAgent.Agent,
            //     flightAgent.Agent,
            //     hotelAgent.Agent,
            //     activityAgent.Agent,
            //     transferAgent.Agent,
            //     referenceAgent.Agent
            //  });

            var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
            .WithHandoffs(coordinatorAgent.Agent, [flightAgent.Agent, hotelAgent.Agent, activityAgent.Agent, transferAgent.Agent, referenceAgent.Agent])      // Coordinator can hand off to all agents
            .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent)
            .WithHandoff(hotelAgent.Agent, coordinatorAgent.Agent)
            .WithHandoff(activityAgent.Agent, coordinatorAgent.Agent)
            .WithHandoff(transferAgent.Agent, coordinatorAgent.Agent)
            .WithHandoff(referenceAgent.Agent, coordinatorAgent.Agent)
            .Build();


            var agent = await workflow.AsAgentAsync("travel-workflow-agent", "Travel Agent Assistant");

            logger.LogInformation("🤖 Travel Agent Assistant created");
            // Start interactive session
            await StartInteractiveChat(agent, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while running the application");
        }
    }

    private static async Task StartInteractiveChat(AIAgent aIAgent, ILogger logger)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        AgentThread agentThread = aIAgent.GetNewThread();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();

            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            try
            {

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();


                // Run agent with user input and agent thread
                var response = await aIAgent.RunAsync(userInput, agentThread);

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

        Console.WriteLine("Goodbye!");
    }
    
}