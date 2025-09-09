using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace SecureWeatherMcpClient;

public class Program
{
    private static readonly HttpClient httpClient = new();
    private static IConfiguration? configuration;
    private static ILogger<Program>? logger;
    private static ChatClient? chatClient;

    public static async Task Main(string[] args)
    {
        // Build configuration
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection and logging
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        logger = host.Services.GetRequiredService<ILogger<Program>>();

        // Initialize Azure OpenAI client
        InitializeAzureOpenAI();

        logger.LogInformation("=== Secure Weather MCP Client ===");
        logger.LogInformation("This client connects to the secure weather MCP server using API key authentication");
        logger.LogInformation("Default API Key: sk-demo-weather-api-key-12345");
        Console.WriteLine();

        // Interactive query loop
        while (true)
        {
            Console.Write("Enter your weather query (or 'exit' to quit): ");
            var query = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(query) || query.ToLower() == "exit")
                break;

            try
            {
                await ProcessQuery(query);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error processing query: {Query}", query);
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("Goodbye!");
    }

    private static void InitializeAzureOpenAI()
    {
        var endpoint = configuration?.GetValue<string>("AzureOpenAI:Endpoint");
        var apiKey = configuration?.GetValue<string>("AzureOpenAI:ApiKey");
        var deploymentName = configuration?.GetValue<string>("AzureOpenAI:DeploymentName");

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(deploymentName))
        {
            logger?.LogError("Azure OpenAI configuration is missing. Please check your appsettings.json file.");
            throw new InvalidOperationException("Azure OpenAI configuration is incomplete");
        }

        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
        chatClient = azureClient.GetChatClient(deploymentName);

        logger?.LogInformation("Azure OpenAI client initialized successfully");
    }

    private static async Task ProcessQuery(string query)
    {
        if (chatClient == null)
        {
            throw new InvalidOperationException("Chat client is not initialized");
        }

        logger?.LogInformation("Processing query: {Query}", query);

        // Get available tools from the secure MCP server
        var tools = await GetAvailableTools();
        var chatTools = ConvertToChatTools(tools);

        // Create chat completion with function calling
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful weather assistant. Use the available weather tools to provide accurate weather information. Always use the tools to get real weather data rather than making up information."),
            new UserChatMessage(query)
        };

        var chatOptions = new ChatCompletionOptions();
        foreach (var tool in chatTools)
        {
            chatOptions.Tools.Add(tool);
        }

        var response = await chatClient.CompleteChatAsync(messages, chatOptions);

        // Handle the response and any tool calls
        await HandleChatResponse(response, messages);
    }

    private static async Task<JsonDocument> GetAvailableTools()
    {
        var serverUrl = configuration?.GetValue<string>("SecureMcpServer:BaseUrl") ?? "http://localhost:5000";
        var apiKey = configuration?.GetValue<string>("SecureMcpServer:ApiKey") ?? "sk-demo-weather-api-key-12345";
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/api/mcp/tools/list");
        request.Headers.Add("X-API-Key", apiKey);

        logger?.LogDebug("Requesting tools from: {Url}", request.RequestUri);

        var response = await httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to get tools: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        logger?.LogDebug("Received tools response: {Content}", content);
        
        return JsonDocument.Parse(content);
    }

    private static List<ChatTool> ConvertToChatTools(JsonDocument toolsDoc)
    {
        var chatTools = new List<ChatTool>();
        
        if (toolsDoc.RootElement.TryGetProperty("tools", out var toolsArray))
        {
            foreach (var tool in toolsArray.EnumerateArray())
            {
                if (tool.TryGetProperty("name", out var nameElement) &&
                    tool.TryGetProperty("description", out var descElement) &&
                    tool.TryGetProperty("inputSchema", out var schemaElement))
                {
                    var functionDefinition = ChatFunctionDefinition.Create(
                        nameElement.GetString()!,
                        descElement.GetString()!,
                        BinaryData.FromString(schemaElement.GetRawText())
                    );
                    
                    chatTools.Add(ChatTool.CreateFunctionTool(functionDefinition));
                }
            }
        }

        logger?.LogInformation("Converted {Count} tools for chat completion", chatTools.Count);
        return chatTools;
    }

    private static async Task HandleChatResponse(ChatCompletion response, List<ChatMessage> messages)
    {
        var responseMessage = response.Content[0];
        
        if (responseMessage.Kind == ChatMessageContentPartKind.Text)
        {
            Console.WriteLine($"Assistant: {responseMessage.Text}");
        }

        // Handle function calls
        if (response.ToolCalls.Count > 0)
        {
            messages.Add(new AssistantChatMessage(response));

            foreach (var toolCall in response.ToolCalls)
            {
                if (toolCall.Kind == ChatToolCallKind.Function)
                {
                    var functionCall = toolCall.FunctionCall;
                    logger?.LogInformation("Executing tool: {ToolName}", functionCall.FunctionName);
                    
                    var result = await ExecuteToolCall(functionCall.FunctionName, functionCall.FunctionArguments);
                    messages.Add(new ToolChatMessage(toolCall.Id, result));
                }
            }

            // Get final response after tool execution
            var finalResponse = await chatClient!.CompleteChatAsync(messages);
            Console.WriteLine($"Assistant: {finalResponse.Content[0].Text}");
        }
    }

    private static async Task<string> ExecuteToolCall(string toolName, BinaryData arguments)
    {
        var serverUrl = configuration?.GetValue<string>("SecureMcpServer:BaseUrl") ?? "http://localhost:5000";
        var apiKey = configuration?.GetValue<string>("SecureMcpServer:ApiKey") ?? "sk-demo-weather-api-key-12345";
        
        var argumentsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(arguments.ToArray());
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/api/mcp/tools/call");
        request.Headers.Add("X-API-Key", apiKey);
        
        var payload = new
        {
            name = toolName,
            arguments = argumentsDict
        };
        
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        logger?.LogDebug("Executing tool call: {ToolName} with arguments: {Arguments}", toolName, arguments.ToString());

        var response = await httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Tool execution failed: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        logger?.LogDebug("Tool execution result: {Result}", responseContent);

        // Extract the text content from the MCP response
        var mcpResponse = JsonDocument.Parse(responseContent);
        if (mcpResponse.RootElement.TryGetProperty("content", out var contentArray) &&
            contentArray.EnumerateArray().FirstOrDefault().TryGetProperty("text", out var textElement))
        {
            return textElement.GetString() ?? responseContent;
        }

        return responseContent;
    }
}
