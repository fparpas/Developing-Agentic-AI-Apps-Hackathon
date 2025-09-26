using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelWithMCP;

class Program
{
    private static Kernel? _kernel;
    private static IClientTransport? _clientTransport;
    private static IMcpClient? _mcpClient;

    static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            var configuration = LoadConfiguration();
            
            // Initialize MCP client
            await InitializeMcpClient(configuration);

            // Create Semantic Kernel with Azure OpenAI
            _kernel = CreateKernel(configuration);

            // Add MCP tools to Semantic Kernel
            await RegisterMcpToolsWithSemanticKernel();

            // Start interactive chat
            await StartInteractiveChat();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Application error: {ex}");
            Console.ResetColor();
        }
        finally
        {
            // Cleanup
            await _mcpClient.DisposeAsync();
        }
    }

    private static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }


    private static async Task InitializeMcpClient(IConfiguration configuration)
    {
        var mcpServerUrl = configuration["MCPServer:RemoteMCP:Endpoint"] ?? throw new InvalidOperationException("Remote MCP endpoint is required");

        _mcpClient = await McpClientFactory.CreateAsync(
           new SseClientTransport(
               new SseClientTransportOptions
               {
                   Endpoint = new Uri(mcpServerUrl),
                   ConnectionTimeout = TimeSpan.FromMinutes(5) // Increase MCP connection timeout to 5 minutes
               }
           )
        );
    }

    private static Kernel CreateKernel(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        var builder = Kernel.CreateBuilder();        
       
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: endpoint,
            apiKey: apiKey
        );

        var kernel = builder.Build();
        
        return kernel;
    }

    private static async Task RegisterMcpToolsWithSemanticKernel()
    {
        if (_mcpClient == null || _kernel == null)
        {
            throw new InvalidOperationException("MCP client or Kernel not initialized");
        }

        var mcpTools = await _mcpClient.ListToolsAsync();
        Console.WriteLine($"Found {mcpTools.Count} MCP tools");

        Console.WriteLine("Available MCP Tools:");
        foreach (var tool in mcpTools)
        {
            Console.WriteLine($"- {tool.Name}: {tool.Description}");
        }

        #pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        //Register MCP tools to kernel
        _kernel.Plugins.AddFromFunctions("WeatherTools", mcpTools.Select(t => t.AsKernelFunction()));
        
        #pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        Console.WriteLine($"Registered {mcpTools.Count} MCP tools with Semantic Kernel");
    }       

    private static async Task StartInteractiveChat()
    {
        Console.WriteLine("\n=== Semantic Kernel with MCP Tools ===");
        Console.WriteLine("You can ask questions and I'll use the available MCP tools to help you.");
        Console.WriteLine("Type 'exit' to quit.\n");

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Create a chat history to maintain conversation context
        var chatHistory = new ChatHistory();
        
        // Add a system message to set the assistant's behavior
        chatHistory.AddSystemMessage("You are a helpful assistant that can use weather tools to provide accurate weather information in US only. Always use the available tools when users ask about weather-related topics.");

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
                // Add user message to chat history
                chatHistory.AddUserMessage(userInput);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Assistant: ");
                Console.ResetColor();

                // Get chat completion service from kernel
                var chatService = _kernel!.GetRequiredService<IChatCompletionService>();
                
                // Get response with chat history context
                var response = await chatService.GetChatMessageContentsAsync(chatHistory, settings, _kernel);
                
                // Get the last message from the response
                var lastMessage = response.LastOrDefault();
                if (lastMessage != null)
                {
                    // Add assistant response to chat history
                    chatHistory.Add(lastMessage);
                    
                    Console.WriteLine(lastMessage.Content);
                }
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
    }
}