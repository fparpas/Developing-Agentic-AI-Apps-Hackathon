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


            // Create AI Agent that tells jokes
            var jokeAgent = await CreateSimpleAgent("JokeAgent", "A simple joke telling agent", "You are a funny assistant that tells jokes.");

            // //TASK 1: Implement current time agent
            // // Create AI Time Agent and register Function tools.
            // // Complete the method to create the Time agent and register the TimeTools.GetCurrentTimeInUTC function as a tool.
            var timeAgent = await CreateTimeAIAgentAndRegisterTools("TimeAgent", "You are a helpful assistant that can provide time information using the available tools.");

            // //TASK 2: Implement MCP client and Weather agent
            // Initialize MCP client
             _mcpClient = await InitializeMcpClient();
            // // Create AI Weather Agent and register MCP Weather tools. 
            // // Complete the method to create the Weather agent and register the MCP Weather tools.
            var weatherAgent = await CreateWeatherAIAgentAndRegisterMCPTools("WeatherAgent", "You are a helpful assistant that can provide weather information using the available tools.");

            // //TASK 3: Implement Agent Service agent
            // // Get the AI Agent Service Agent
            // //Complete the method to create the Agent Service agent.
            var agentServiceAgent = await CreateAIAgentServiceAgent("AgentServiceAgent", "You are a helpful assistant that can provide agent service information using the available tools.");

            // Start interactive chat
            await StartInteractiveChat(jokeAgent);
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

        //Implement your code here to initialize the MCP client.

        throw new NotImplementedException("Method not implemented");

        return null; //return the mcpClient
    }

    private static async Task<AIAgent> CreateSimpleAgent(string agentName, string description, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

       // Create the chat client and agent, and provide the function tool to the agent.
        AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .CreateAIAgent(name: agentName, instructions: instructions, description: description);

        return agent;
    } 

    private static async Task<AIAgent> CreateWeatherAIAgentAndRegisterMCPTools(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        //Implement your code here to create an AI agent and register the MCP Weather tools.

        throw new NotImplementedException("Method not implemented");

        return null; //return agent
    }  

    private static async Task<AIAgent> CreateTimeAIAgentAndRegisterTools(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        //Implement your code here to create an AI agent and register the TimeTools.GetCurrentTimeInUTC function as a tool.

        throw new NotImplementedException("Method not implemented");

        return null; //return agent
    }  

    private static async Task<AIAgent> CreateAIAgentServiceAgent(string agentName, string instructions)
    {
        var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var apiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");

        var agentServiceEndpoint = _configuration["AgentService:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI agent service endpoint is required");
        var agentServiceId = _configuration["AgentService:AgentId"] ?? throw new InvalidOperationException("Azure OpenAI agent service identity is required");

        // Create PersistentAgentsClient to connect to the Agent Service

        throw new NotImplementedException("Method not implemented");

        return null; //return agent
    } 
    private static async Task StartInteractiveChat(AIAgent aIAgent)
    {
        Console.WriteLine($"\n=== Interactive Session for Agent: {aIAgent.Name} started===");
        Console.WriteLine($"Agent Description: {aIAgent.Description}");
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