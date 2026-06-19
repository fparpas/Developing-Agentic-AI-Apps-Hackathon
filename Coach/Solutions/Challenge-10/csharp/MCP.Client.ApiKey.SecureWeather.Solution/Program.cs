using Azure.AI.OpenAI;
using System.Text.Json;
using System.ClientModel;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Options;


class Program
{
    public static AzureOpenAIClient openAIClient = null;
    public static IClientTransport clientTransport = null;
    public static IMcpClient mcpClient = null;

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
                clientTransport = new SseClientTransport(new SseClientTransportOptions()
                {
                    Endpoint = new Uri(remoteMCP),
                    AdditionalHeaders = new Dictionary<string, string>
                    {
                        { "X-API-Key", "SuperSecureSecretUsedAsApiKey1" }
                    }
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
            using var factory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            IChatClient client = new ChatClientBuilder(
                                    new AzureOpenAIClient(new Uri(endpoint),
                                    new ApiKeyCredential(apiKey))
                                .GetChatClient(deploymentName).AsIChatClient())
                                .UseLogging(factory)
                                .UseFunctionInvocation()
                                .Build();
            //initialize Azure Open AI client

            List<ChatMessage> messages = new();
            messages.Add(new(ChatRole.Assistant, "You are an AI weather assistant."));
            while (true)
            {
                Console.Write("Prompt: ");
                messages.Add(new(ChatRole.User, Console.ReadLine()));

                List<ChatResponseUpdate> updates = [];
                var response = await client.GetResponseAsync(messages, new() { Tools = [.. tools] });

                Console.WriteLine(response.Text);
                Console.WriteLine();
                messages.AddMessages(updates);
            }
            #endregion
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application error: {ex.ToString()}");
        }
    }
}