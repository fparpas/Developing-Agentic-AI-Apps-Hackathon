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
   const string SourceName = "WorkflowSample";
    const string ServiceName = "AgentOpenTelemetry";
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
        var otlpEndpoint = configuration["Observability:AspireUrl"];

        // Create a resource to identify this service
        var resource = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, serviceVersion: "1.0.0")
            .AddAttributes(new Dictionary<string, object>
            {
                ["service.instance.id"] = Environment.MachineName,
                ["deployment.environment"] = "development"
            })
            .Build();

    // Setup tracing with resource
    var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, serviceVersion: "1.0.0"))
        .AddSource(SourceName) // Our custom activity source
        .AddSource("*Microsoft.Agents.AI") // Agent Framework telemetry
        .AddSource("Microsoft.Agents.AI*") // Agent Framework telemetry
        .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry
        .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry
        .AddHttpClientInstrumentation() // Capture HTTP calls to OpenAI
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        // Setup metrics with resource and instrument name filtering
        using var tracerProvider = tracerProviderBuilder.Build();
    // Setup metrics with resource and instrument name filtering
    // using var meterProvider = Sdk.CreateMeterProviderBuilder()
    //     .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, serviceVersion: "1.0.0"))
    //     .AddMeter(SourceName) // Our custom meter
    //     .AddMeter("*Microsoft.Agents.AI") // Agent Framework metrics
    //     .AddMeter("Microsoft.Agents.AI*") // Agent Framework telemetry
    //     .AddHttpClientInstrumentation() // HTTP client metrics
    //     .AddRuntimeInstrumentation() // .NET runtime metrics
    //     .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint))
    //     .Build();
        // Build host
        builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<IConfiguration>(configuration);
        builder.Services.AddSingleton<McpClientService>();
        // builder.Services.AddLogging(loggingBuilder =>
        // {
        //     loggingBuilder.AddConsole();
        //     loggingBuilder.SetMinimumLevel(LogLevel.Information);
        // });

        // Setup structured logging with OpenTelemetry
        // var serviceCollection = new ServiceCollection();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder
            .SetMinimumLevel(LogLevel.Debug)
            .AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName, serviceVersion: "1.0.0"));
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

            // await StartInteractiveChat(flightAgent.Agent);
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

            // var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
            // .WithHandoffs(coordinatorAgent.Agent, [flightAgent.Agent, hotelAgent.Agent, activityAgent.Agent, transferAgent.Agent, referenceAgent.Agent])      // Coordinator can hand off to all agents
            // .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent)
            // .WithHandoff(hotelAgent.Agent, coordinatorAgent.Agent)
            // .WithHandoff(activityAgent.Agent, coordinatorAgent.Agent)
            // .WithHandoff(transferAgent.Agent, coordinatorAgent.Agent)
            // .WithHandoff(referenceAgent.Agent, coordinatorAgent.Agent)
            // .Build();


            // var agent = await workflow.AsAgentAsync("travel-workflow-agent", "Travel Agent Assistant");

            logger.LogInformation("🤖 Travel Agent Assistant created");
            // Start interactive session
            await StartInteractiveChat(serviceProvider.Services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while running the application");
        }
    }

    private static async Task StartInteractiveChat(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n=== Agent Framework with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        // var aIAgent = await workflow.AsAgentAsync("travel-workflow-agent", "Travel Agent Assistant");
        // AgentThread agentThread = aIAgent.GetNewThread();
        List<ChatMessage> messages = new();
        StreamingRun run = null;
        string userInput = String.Empty;

        var flightAgent = services.GetRequiredService<FlightAgent>();
        // var hotelAgent = services.GetRequiredService<HotelAgent>();
        // var activityAgent = services.GetRequiredService<ActivityAgent>();
        // var transferAgent = services.GetRequiredService<TransferAgent>();
        // var referenceAgent = services.GetRequiredService<ReferenceAgent>();
        var coordinatorAgent = services.GetRequiredService<TravelCoordinatorAgent>();


        //         var workflowBuilder = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
        // .WithHandoffs(coordinatorAgent.Agent, new[] { flightAgent.Agent, hotelAgent.Agent, activityAgent.Agent, transferAgent.Agent, referenceAgent.Agent })      // Coordinator can hand off to all agents
        // .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent)
        // .WithHandoff(hotelAgent.Agent, coordinatorAgent.Agent)
        // .WithHandoff(activityAgent.Agent, coordinatorAgent.Agent)
        // .WithHandoff(transferAgent.Agent, coordinatorAgent.Agent)
        // .WithHandoff(referenceAgent.Agent, coordinatorAgent.Agent);


//         var workflowBuilder = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
// .WithHandoffs(coordinatorAgent.Agent, new[] { flightAgent.Agent })      // Coordinator can hand off to all agents
// .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent);

        var workflowBuilder = AgentWorkflowBuilder.CreateHandoffBuilderWith(coordinatorAgent.Agent) // Start with the coordinator agent
        .WithHandoffs(coordinatorAgent.Agent, new[] { flightAgent.Agent })      // Coordinator can hand off to all agents
        .WithHandoff(flightAgent.Agent, coordinatorAgent.Agent);
        var workflowAgent = await workflowBuilder.Build().AsAgentAsync("TravelAgent", "TravelAgent");
var workflowAgentThread = workflowAgent.GetNewThread();
        while (true)
        {
            // var workflow = workflowBuilder.Build();
            

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("User: ");
            Console.ResetColor();
            // var workflowAgentThread = workflowAgent.GetNewThread();
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

                // var workflowAgentThread = workflowAgent.GetNewThread();

                var result = string.Empty;

                workflowAgent = await workflowBuilder.Build().AsAgentAsync("TravelAgent", "TravelAgent");
                workflowAgentThread = workflowAgent.GetNewThread();
                
                // await foreach (var update in workflowAgent.RunStreamingAsync(messages, workflowAgentThread).ConfigureAwait(true))
                // {
                //     result += update;
                //     Console.Write(update);
                // }
                var response = await workflowAgent.RunAsync(messages, workflowAgentThread);
                Console.WriteLine(response);
                messages.AddRange(response.Messages);
                // run = await InProcessExecution.StreamAsync(workflow, messages);
                // await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

                // List<ChatMessage> newMessages = new();
                // await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(true))
                // {
                //     Console.WriteLine($"{evt.GetType().Name}: {evt.Data}");
                //     if (evt is AgentRunUpdateEvent e)
                //     {
                //         // Console.WriteLine($"{e.ExecutorId}: {e.Data}");
                //     }
                //     else if (evt is WorkflowOutputEvent completed)
                //     {
                //         newMessages = (List<ChatMessage>)completed.Data!;
                //         break;
                //     }
                //     else if (evt is ExecutorFailedEvent failed)
                //     {
                //         Console.ForegroundColor = ConsoleColor.Red;
                //         Console.WriteLine($"Error: {failed.Data}");
                //         Console.ResetColor();
                //         Console.WriteLine();
                //     }
                // }
                // await run.RunToCompletionAsync();

                // Add new messages to conversation history
                // messages.AddRange(newMessages.Skip(messages.Count));

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
    private static async Task StartInteractiveChat(AIAgent aIAgent)
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
    }
    
}