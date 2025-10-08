using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;


namespace SemanticKernelWithMCP;

class Program
{
    private static IConfiguration _configuration;
    private static IClientTransport? _clientTransport;
    private static IMcpClient? _mcpClient;

    static async Task Main(string[] args)
    {
        try
        {
            // Load configuration
            _configuration = LoadConfiguration();

            // Initialize MCP client
            _mcpClient = await InitializeMcpClient();

            // Create AI Time Agent and register Function tools. 
            var timeAgent = await CreateTimeAIAgentAndRegisterTools("TimeAgent", "You are a helpful assistant that can provide time information using the available tools.");

            // Create AI Weather Agent and register MCP Weather tools. 
            var weatherAgent = await CreateWeatherAIAgentAndRegisterMCPTools("WeatherAgent", "You are a helpful assistant that can provide weather information using the available tools.");

            // Create AI Agent Service Agent
            var agentServiceAgent = await CreateAIAgentServiceAgent("AgentServiceAgent", "You are a helpful assistant that can provide agent service information using the available tools.");

            // Start interactive chat
            // await StartInteractiveChat(timeAgent);
            // await StartInteractiveChat(weatherAgent);
            await StartInteractiveChat(agentServiceAgent);
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


    private static async Task<IMcpClient> InitializeMcpClient()
    {
        var mcpServerUrl = _configuration["MCPServer:RemoteMCP:Endpoint"] ?? throw new InvalidOperationException("Remote MCP endpoint is required");

        _mcpClient = await McpClientFactory.CreateAsync(
           new SseClientTransport(
               new SseClientTransportOptions
               {
                   Endpoint = new Uri(mcpServerUrl),
                   ConnectionTimeout = TimeSpan.FromMinutes(5) // Increase MCP connection timeout to 5 minutes
               }
           )
        );

        return _mcpClient;
    }

    private static async Task<AIAgent> CreateWeatherAIAgentAndRegisterMCPTools(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        var mcpTools = await _mcpClient.ListToolsAsync();
        Console.WriteLine($"Found {mcpTools.Count} MCP tools");

        Console.WriteLine("Available MCP Tools:");
        foreach (var tool in mcpTools)
        {
            Console.WriteLine($"- {tool.Name}: {tool.Description}");
        }

        AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .CreateAIAgent(
                instructions: instructions,
                name: agentName,
                tools: [.. mcpTools.Cast<AITool>().ToList(), AIFunctionFactory.Create(TimeTools.GetCurrentTimeInUTC)]
            );

        return agent;
    }  

    private static async Task<AIAgent> CreateTimeAIAgentAndRegisterTools(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");


        AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .CreateAIAgent(
                instructions: instructions,
                name: agentName,
                tools: [AIFunctionFactory.Create(TimeTools.GetCurrentTimeInUTC)]
            );

        return agent;
    }  

    private static async Task<AIAgent> CreateAIAgentServiceAgent(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        var agentServiceEndpoint = _configuration["AgentService:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI agent service endpoint is required");
        var agentServiceId = _configuration["AgentService:AgentId"] ?? throw new InvalidOperationException("Azure OpenAI agent service identity is required");

        var persistentAgentsClient = new PersistentAgentsClient(agentServiceEndpoint, new DefaultAzureCredential());

        // Retrieve the agent that was just created as an AIAgent using its ID
        AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentServiceId);

        return agent;
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

        Console.WriteLine("Goodbye!");
    }
}