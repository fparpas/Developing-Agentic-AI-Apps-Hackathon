using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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

        // Build host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<McpClientService>();
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        var serviceProvider = host.Services;
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var mcpClient = serviceProvider.GetRequiredService<McpClientService>();

        try
        {
            logger.LogInformation("🌟 Welcome to Travel Multi-Agent Assistant! 🌟");

            // Create Semantic Kernel
            var builder = Kernel.CreateBuilder();

            // Configure Azure OpenAI
            var azureOpenAiApiKey = configuration["AzureOpenAI:ApiKey"];
            var azureOpenAiEndpoint = configuration["AzureOpenAI:Endpoint"];

            if (!string.IsNullOrEmpty(azureOpenAiApiKey) && !string.IsNullOrEmpty(azureOpenAiEndpoint))
            {
                builder.AddAzureOpenAIChatCompletion(
                    configuration["AzureOpenAI:ModelId"] ?? "gpt-4o",
                    azureOpenAiEndpoint,
                    azureOpenAiApiKey);
                logger.LogInformation("Using Azure OpenAI");
            }            // 
            else
            {
                logger.LogError("❌ No valid AI service configuration found. Please configure OpenAI or Azure OpenAI in appsettings.json or user secrets.");
                return;
            }

            var kernel = builder.Build();

            var mcpTools = await mcpClient.GetMcpToolsAsync();
            // Create specialized agents
            var flightAgent = new FlightAgent(kernel, mcpTools, serviceProvider.GetRequiredService<ILogger<FlightAgent>>());
            var hotelAgent = new HotelAgent(kernel, mcpTools, serviceProvider.GetRequiredService<ILogger<HotelAgent>>());
            var activityAgent = new ActivityAgent(kernel, mcpTools, serviceProvider.GetRequiredService<ILogger<ActivityAgent>>());
            var transferAgent = new TransferAgent(kernel, mcpTools, serviceProvider.GetRequiredService<ILogger<TransferAgent>>());
            var referenceAgent = new ReferenceAgent(kernel, mcpTools, serviceProvider.GetRequiredService<ILogger<ReferenceAgent>>());
            var coordinatorAgent = new TravelCoordinatorAgent(kernel, serviceProvider.GetRequiredService<ILogger<TravelCoordinatorAgent>>());

            logger.LogInformation("✅ All agents initialized successfully");

            // Create orchestrated agent group chat with coordinator leading
            var chat = new AgentGroupChat(coordinatorAgent.Agent, flightAgent.Agent, hotelAgent.Agent, activityAgent.Agent, transferAgent.Agent, referenceAgent.Agent)
            {
                ExecutionSettings = new()
                {
                    TerminationStrategy = new TravelTerminationStrategy()
                    {
                        MaximumIterations = 50,
                        CoordinatorAgentName = coordinatorAgent.Agent.Name ?? "TravelCoordinatorAgent"
                    }
                }
            };

            logger.LogInformation("🤖 Travel Agent Assistant created with coordinator and 5 specialized agents");            // Start interactive session
            await RunInteractiveSession(chat, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ An error occurred while running the application");
        }
    }

    private static async Task RunInteractiveSession(AgentGroupChat chat, ILogger logger)
    {
        logger.LogInformation("\n🎯 Travel Assistant Ready! Type your travel request or 'exit' to quit.\n");

        string? userInput;
        while ((userInput = GetUserInput()) != null && userInput.ToLower() != "exit")
        {
            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            try
            {
                logger.LogInformation($"\n🔄 Processing your request: {userInput}");
                
                // Add user message to chat
                chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userInput));

                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("🤖 TRAVEL AGENTS WORKING ON YOUR REQUEST...");
                Console.WriteLine(new string('=', 60));

                // Process the conversation
                await foreach (var content in chat.InvokeAsync())
                {
                    Console.WriteLine($"\n🗣️  {content.AuthorName ?? "System"}:");
                    Console.WriteLine($"💬 {content.Content}");
                    Console.WriteLine(new string('-', 40));
                }

                Console.WriteLine("\n✅ Request processed! Ask another question or type 'exit' to quit.\n");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error processing request");
                Console.WriteLine($"❌ Sorry, I encountered an error: {ex.Message}");
            }
        }

        logger.LogInformation("👋 Thank you for using Travel Multi-Agent Assistant!");
    }

    private static string? GetUserInput()
    {
        Console.Write("✈️  You: ");
        return Console.ReadLine();
    }
}

// Custom termination strategy for travel agent conversations
public class TravelTerminationStrategy : TerminationStrategy
{
    public new int MaximumIterations { get; set; } = 50;
    public string CoordinatorAgentName { get; set; } = "TravelCoordinatorAgent";

    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken = default)
    {
        // Terminate when max iterations reached
        if (history.Count >= MaximumIterations)
            return Task.FromResult(true);

        var lastMessage = history.LastOrDefault();
        
        // Terminate when coordinator provides final response with specific keywords
        if (lastMessage?.AuthorName == CoordinatorAgentName && lastMessage.Content != null)
        {
            var content = lastMessage.Content.ToLowerInvariant();
            if (content.Contains("final recommendations") || 
                content.Contains("complete travel plan") ||
                content.Contains("booking summary") ||
                content.Contains("is there anything else") ||
                content.Contains("hope this helps"))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}