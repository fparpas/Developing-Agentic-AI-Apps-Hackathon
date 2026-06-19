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

        builder.Services.AddSingleton(configurationService.Configuration);
        builder.Services.AddSingleton<McpClientService>();
  
        // Add a chat client to the service collection.
        builder.Services.AddSingleton<IChatClient>(new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient());

        // Add a persistent agents client to the service collection. The Persistent Agents Client is used to interact with Foundry Agent Service.
        // For local development, use AzureCliCredential (requires 'az login')
        builder.Services.AddSingleton(new PersistentAgentsClient(
            foundryEndpoint,
            new DefaultAzureCredential()));

        //Get required services
        var chatClient = builder.Services.BuildServiceProvider().GetRequiredService<IChatClient>();
        var persistentAgentsClient = builder.Services.BuildServiceProvider().GetRequiredService<PersistentAgentsClient>();
        var mcpClientService = builder.Services.BuildServiceProvider().GetRequiredService<McpClientService>();
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

        // Register agents
        builder.Services.AddSingleton<FlightAgent>();
        builder.Services.AddSingleton<HotelAgent>();
        builder.Services.AddSingleton<ActivityAgent>();
        builder.Services.AddSingleton<TransferAgent>();
        builder.Services.AddSingleton<ReferenceAgent>();
        builder.Services.AddSingleton<CoordinatorAgent>();
        builder.Services.AddSingleton(new TravelPolicyAgent(persistentAgentsClient, foundryAgentId));

        //Get agents
        var flightAgent = builder.Services.BuildServiceProvider().GetRequiredService<FlightAgent>();
        var hotelAgent = builder.Services.BuildServiceProvider().GetRequiredService<HotelAgent>();
        var activityAgent = builder.Services.BuildServiceProvider().GetRequiredService<ActivityAgent>();
        var transferAgent = builder.Services.BuildServiceProvider().GetRequiredService<TransferAgent>();
        var referenceAgent = builder.Services.BuildServiceProvider().GetRequiredService<ReferenceAgent>();
        var coordinatorAgent = builder.Services.BuildServiceProvider().GetRequiredService<CoordinatorAgent>();
        var travelPolicyAgent = builder.Services.BuildServiceProvider().GetRequiredService<TravelPolicyAgent>();

        #region Sequential Workflow
        // Create a sequential workflow 
        var sequentialWorkflow = AgentWorkflowBuilder.BuildSequential(new[]
            {
                coordinatorAgent.Agent,
                flightAgent.Agent,
                hotelAgent.Agent,
                activityAgent.Agent,
                transferAgent.Agent,
                referenceAgent.Agent
             });
        #endregion

        #region Parallel Workflow
        // Create a concurrent workflow with all agents
        var concurrentWorkflow = AgentWorkflowBuilder.BuildConcurrent(new[]
        {
                coordinatorAgent.Agent,
                flightAgent.Agent,
                hotelAgent.Agent,
                activityAgent.Agent,
                transferAgent.Agent,
                referenceAgent.Agent
             });
        #endregion

        #region Handoff Workflow
        // Create a handoff workflow where the coordinator can hand off to any other agent, and
        var handOffWorkflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
        .WithHandoffs(coordinatorAgent.Agent, new[] { flightAgent.Agent, hotelAgent.Agent, activityAgent.Agent, transferAgent.Agent, referenceAgent.Agent })      // Coordinator can hand off to all agents
        .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent)
        .WithHandoff(hotelAgent.Agent, coordinatorAgent.Agent)
        .WithHandoff(activityAgent.Agent, coordinatorAgent.Agent)
        .WithHandoff(transferAgent.Agent, coordinatorAgent.Agent)
        .WithHandoff(referenceAgent.Agent, coordinatorAgent.Agent)
        .Build();
        #endregion
        
        #region Agents as Tools
        // Initialize agents and add them in array to use them as tools for the main orchestrator agent
        AIAgent[] agentsAsTools = new AIAgent[]
        {
            travelPolicyAgent.Agent,
            flightAgent.Agent,
            hotelAgent.Agent,
            activityAgent.Agent,
            transferAgent.Agent,
            referenceAgent.Agent
        };

        // Create and register the main orchestrator agent, add the other agents as tools
        var mainOrchestratorAgent = new OrchestratorAgent(
            builder.Services.BuildServiceProvider().GetRequiredService<IChatClient>(),
            agentsAsTools);
        // Register the main orchestrator agent
        builder.Services.AddSingleton(mainOrchestratorAgent);
        // Get the main orchestrator agent
        var orchestratorAgent = builder.Services.BuildServiceProvider().GetRequiredService<OrchestratorAgent>();
        #endregion

        // Build service provider
        var serviceProvider = builder.Build();        

        try
        {
            logger.LogInformation("🌟 Welcome to Travel Multi-Agent Assistant! 🌟");            

            // await StartInteractiveChat(flightAgent.Agent);
            logger.LogInformation("✅ All agents initialized successfully");

            logger.LogInformation("🤖 Travel Agent Assistant created");
            
            // Start interactive session
            // // Start with the main orchestrator agent and use agents as tools pattern
            // await StartInteractiveChat(orchestratorAgent.Agent);
            // // Alternatively, start with sequential workflow pattern
            // await StartInteractiveChat(sequentialWorkflow);
            // // Alternatively, start with concurrent workflow pattern
            // await StartInteractiveChat(concurrentWorkflow);
            // Alternatively, start with handoff workflow pattern

            await StartInteractiveChat(handOffWorkflow);
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
    
    private static async Task StartInteractiveChat(Workflow workflow)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        List<ChatMessage> messages = new();
        string userInput = String.Empty;

        while (true)
        {          

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();

            // Read user input
            userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            try
            {
                messages.Add(new(ChatRole.User, userInput));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                var result = string.Empty;

                // Execute workflow and process events
                await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages).ConfigureAwait(false);
                await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
                
                List<ChatMessage> newMessages = new();
                await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
                {
                    if (evt is AgentRunUpdateEvent e)
                    {
                        // Console.WriteLine($"{e.ExecutorId}: {e.Data}");
                        Console.Write(e.Data);
                    }
                    else if (evt is WorkflowOutputEvent completed)
                    {
                        newMessages = (List<ChatMessage>)completed.Data!;
                        break;
                    }
                    else if (evt is ExecutorFailedEvent failed)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {failed.Data}");
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                    //  Console.WriteLine($"{evt.GetType().Name}: {evt.Data}");
                }

                // Add new messages to conversation history
                messages.AddRange(newMessages.Skip(messages.Count));              
                Console.WriteLine();
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
        Console.ReadLine();
    }
}