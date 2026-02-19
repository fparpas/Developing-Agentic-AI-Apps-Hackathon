using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.AI.Projects;
using Azure.Identity;
using Azure;
using Azure.AI.Projects.OpenAI;
using OpenAI.Responses;

namespace AgentServiceFileSearch;

#pragma warning disable OPENAI001

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

        // Build host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConfiguration>(configuration);
            })
            .Build();

        try
        {
            Console.WriteLine("🔍 Azure Agent Service File Search Console Application");
            Console.WriteLine("===============================================");

            await RunAgentConversation(configuration["AgentService:Endpoint"], configuration["AgentService:AgentName"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ An error occurred while running the application: {ex.Message}");
        }
    }
private static async Task RunAgentConversation(string projectEndpoint, string agentName)
    {
        var endpoint = new Uri(projectEndpoint);
        AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

        // Optional Step: Create a conversation to use with the agent
        ProjectConversation conversation = projectClient.OpenAI.Conversations.CreateProjectConversation();

        AgentRecord agentRecord = projectClient.Agents.GetAgent(agentName);
        Console.WriteLine($"Agent retrieved (name: {agentRecord.Name}, id: {agentRecord.Id})");

        // Get the response client for the agent and conversation
        ProjectResponsesClient responsesClient = projectClient.OpenAI.GetProjectResponsesClientForAgent(defaultAgent: agentName, defaultConversationId: conversation.Id);


        await RunInteractiveSessionAsync(responsesClient);
    }

    private static async Task RunAgentConversation(string userMessage, ProjectResponsesClient responsesClient)
    {

        ResponseResult response = responsesClient.CreateResponse(userMessage);
        Console.Write(response.GetOutputText());
    }   
    
    private static async Task RunInteractiveSessionAsync(ProjectResponsesClient responsesClient)
    {
        Console.WriteLine("\n🎯 Agent Service Ready! Enter your search queries or 'exit' to quit.");
        Console.WriteLine("💡 Commands:");
        Console.WriteLine("   - Enter your query to search for information in the files");
        Console.WriteLine("   - 'exit' - Quit application\n");

        string? userInput;

        while ((userInput = GetUserInput()) != null && userInput.ToLower() != "exit")
        {
            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            try
            {
                await RunAgentConversation(userInput, responsesClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sorry, I encountered an error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private static string? GetUserInput()
    {
        Console.Write("🔍 User Search> ");
        return Console.ReadLine();
    }

}