using Azure.AI.OpenAI;
using System.Text.Json;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;


class Program
{
    public static ChatClient chatClient = null;
    public static AzureOpenAIClient openAIClient = null;
    public static IClientTransport clientTransport = null;
    public static IMcpClient mcpClient = null;
    public static List<ChatTool> availableTools = new List<ChatTool>();
    public static ChatCompletionOptions chatCompletionOptions = null;

    public static async Task Main(string[] args)
    {
        try
        {
            #region Load Configuration

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Load Azure Open AI settings
            var endpoint = configuration["AzureOpenAI:Endpoint"] ?? "Enter value here if not set in app.settings";
            var apiKey = configuration["AzureOpenAI:ApiKey"] ?? "Enter value here if not set in app.settings";
            var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? "Enter value here if not set in app.settings";

            //Load MCP settings
            var useLocalMCP = configuration.GetValue<bool>("MCPServer:UseLocalMCP", true);
            var remoteMCP = configuration.GetValue<string>("MCPServer:RemoteMCP:Endpoint", "Enter value here if not set in app.settings");
            var localMCPName = configuration.GetValue<string>("MCPServer:LocalMCP:Name", "Enter value here if not set in app.settings");
            var localMCPCommand = configuration.GetValue<string>("MCPServer:LocalMCP:Command", "Enter value here if not set in app.settings");
            var localMCPArguments = configuration.GetSection("MCPServer:LocalMCP:Arguments").Get<string[]>() ?? Array.Empty<string>();

            //Validate Azure Open AI settings
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(deploymentName))
            {
                Console.WriteLine("Warning: Azure OpenAI configuration is incomplete. Please update appsettings.json");
                Console.WriteLine("You can also embed the settings directly into the code above.");

                throw new InvalidOperationException("Azure OpenAI configuration is missing. Please check appsettings.json");
            }

            //Validate MCP settings
            if (useLocalMCP)
            {
                if (string.IsNullOrEmpty(localMCPName) || string.IsNullOrEmpty(localMCPCommand) || localMCPArguments.Length == 0)
                {
                    Console.WriteLine("Warning: Local MCP configuration is incomplete. Please update appsettings.json");
                    Console.WriteLine("You can also embed the settings directly into the code above.");

                    throw new InvalidOperationException("Local MCP configuration is missing. Please check appsettings.json");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(remoteMCP))
                {
                    Console.WriteLine("Warning: Remote MCP configuration is incomplete. Please update appsettings.json");
                    Console.WriteLine("You can also embed the settings directly into the code above.");

                    throw new InvalidOperationException("Remote MCP configuration is missing. Please check appsettings.json");
                }
            }
            #endregion

            #region Initialize Azure OpenAI and MCP Client and get MCP tools

            if (useLocalMCP)
            {
                clientTransport = new StdioClientTransport(new()
                {
                    Name = localMCPName,
                    Command = localMCPCommand,
                    Arguments = localMCPArguments,
                });
            }
            else
            {
                // Make sure that Weather MCP server is running
                clientTransport = new SseClientTransport(new()
                {
                    Endpoint = new Uri(remoteMCP)
                });
            }

            //Initialize MCP client
            mcpClient = await McpClientFactory.CreateAsync(clientTransport!);

            //Load MCP tools from server
            var tools = await mcpClient.ListToolsAsync();

            //Write in console the name of the tools
            Console.WriteLine("Available MCP Tools:");
            foreach (var tool in tools)
            {
                Console.WriteLine($"- {tool.Name}");
            }

            // Add tools schema to chat completion
            chatCompletionOptions = AddToolsSchemaToChat(tools);

            Console.WriteLine($"Converted {chatCompletionOptions.Tools.Count} MCP tools to Azure OpenAI tools.");

            //initialize Azure Open AI client
            openAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            chatClient = openAIClient.GetChatClient(deploymentName);

            await QueryProcessing();
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex.ToString()}");
        }
    }

    private static ChatCompletionOptions AddToolsSchemaToChat(IList<McpClientTool> tools)
    {
        var options = new ChatCompletionOptions()
        {
            ToolChoice = ChatToolChoice.CreateAutoChoice()
        };

        foreach (var tool in tools)
        {
            // Convert JsonElement? to string, with fallback for null/empty schema
            var schemaString = tool.JsonSchema.GetRawText() ?? "{}";

            // Convert MCP tool to Azure OpenAI ChatTool with basic schema
            var chatTool = ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description ?? tool.Name,
                functionParameters: BinaryData.FromString(schemaString)
            );
            options.Tools.Add(chatTool);
        }

        return options;
    }

    static void PromptForInput()
    {
        Console.WriteLine("Enter a command (or 'exit' to quit):");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("> ");
        Console.ResetColor();
    }

    public static async Task QueryProcessing()
    {
        PromptForInput();
        while (Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                PromptForInput();
                continue;
            }
            // Call Azure OpenAI Chat client and get response
            try
            {
                var messages = new List<ChatMessage>()
                {
                    new SystemChatMessage("You are a helpful weather provider assistant. Use the available tools to get real weather data when users ask about weather."),
                    new UserChatMessage(query),
                };

                var response = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

                // Handle the response and any tool calls
                await HandleChatResponse(response.Value, messages);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error calling Azure OpenAI: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine();
            PromptForInput();
        }
    }

    private static async Task HandleChatResponse(ChatCompletion completion, List<ChatMessage> messages)
    {
        try
        {
            // Display assistant's response if there's text content
            if (completion.Content.Count > 0 && !string.IsNullOrEmpty(completion.Content[0].Text ?? ""))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Assistant: {completion.Content[0].Text ?? ""}");
                Console.ResetColor();
            }

            // Handle tool calls if any
            if (completion.ToolCalls.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Executing tools...");
                Console.ResetColor();

                // Add assistant message with tool calls to conversation first                
                AssistantChatMessage assistantMessage = null;
                if (completion.Content.Count > 0)
                {
                    assistantMessage = new AssistantChatMessage(completion.Content[0].Text ?? "");
                }
                else
                {
                    assistantMessage = new AssistantChatMessage("");
                }

                foreach (var toolCall in completion.ToolCalls)
                {
                    assistantMessage.ToolCalls.Add(toolCall);
                }                            
                messages.Add(assistantMessage);

                // Execute each tool call and add tool responses
                foreach (var toolCall in completion.ToolCalls)
                {
                    if (toolCall.Kind == ChatToolCallKind.Function)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Calling function: {toolCall.FunctionName}");
                        Console.ResetColor();

                        var result = await ExecuteMcpTool(toolCall.FunctionName, toolCall.FunctionArguments);
                        messages.Add(new ToolChatMessage(toolCall.Id, result));
                    }
                }

                // Get final response after tool execution
                var finalResponse = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
                await HandleChatResponse(finalResponse.Value, messages);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error handling chat response: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static async Task<string> ExecuteMcpTool(string toolName, BinaryData arguments)
    {
        try
        {
            Console.WriteLine($"Executing MCP tool: {toolName}");
            
            // Parse arguments from Azure OpenAI format
            var args = JsonSerializer.Deserialize<Dictionary<string, object>>(arguments.ToArray()) ?? new Dictionary<string, object>();
            
            // Convert to IReadOnlyDictionary<string, object?> for the MCP call
            IReadOnlyDictionary<string, object?> mcpArgs = args.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
            
            // Call the MCP tool
            var result = await mcpClient.CallToolAsync(toolName, mcpArgs);
            
            // Return the result as a string
            var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

            Console.ForegroundColor = ConsoleColor.Yellow;                        
            Console.WriteLine($"Tool result: {resultJson}");
            Console.ResetColor();

            return resultJson;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error executing tool {toolName}: {ex.Message}";
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
            return errorMessage;
        }
    }
}