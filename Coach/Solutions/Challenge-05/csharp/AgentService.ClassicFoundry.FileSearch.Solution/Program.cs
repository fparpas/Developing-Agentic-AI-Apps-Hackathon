using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.AI.Projects;
using Azure.Identity;
using Azure.AI.Agents.Persistent;
using Azure;

namespace AgentServiceFileSearch;

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

            await RunAgentConversation(configuration["AgentService:Endpoint"], configuration["AgentService:AgentId"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ An error occurred while running the application: {ex.Message}");
        }
    }
private static async Task RunAgentConversation(string projectEndpoint, string agentId)
    {
        var endpoint = new Uri(projectEndpoint);
        AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

        PersistentAgentsClient agentsClient = projectClient.GetPersistentAgentsClient();

        PersistentAgent agent = agentsClient.Administration.GetAgent(agentId);

        PersistentAgentThread thread = agentsClient.Threads.CreateThread();
        Console.WriteLine($"Created thread, ID: {thread.Id}");

        await RunInteractiveSessionAsync(agentsClient, agent, thread);
    }

    private static async Task RunAgentConversation(string userMessage, PersistentAgentsClient agentsClient, PersistentAgent agent, PersistentAgentThread thread)
    {
        PersistentThreadMessage messageResponse = agentsClient.Messages.CreateMessage(
                thread.Id,
                MessageRole.User,
                userMessage);

        ThreadRun run = agentsClient.Runs.CreateRun(
            thread.Id,
            agent.Id);

        // Poll until the run reaches a terminal status
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = agentsClient.Runs.GetRun(thread.Id, run.Id);
        }
        while (run.Status == RunStatus.Queued
            || run.Status == RunStatus.InProgress);
        if (run.Status != RunStatus.Completed)
        {
            throw new InvalidOperationException($"Run failed or was canceled: {run.LastError?.Message}");
        }

        Pageable<PersistentThreadMessage> messages = agentsClient.Messages.GetMessages(
            thread.Id, order: ListSortOrder.Ascending);

        // Display messages
        foreach (PersistentThreadMessage threadMessage in messages)
        {
            Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - Thread History - {threadMessage.Role}: ");
            foreach (MessageContent contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                {
                    Console.Write(textItem.Text);
                }
                else if (contentItem is MessageImageFileContent imageFileItem)
                {
                    Console.Write($"<image from ID: {imageFileItem.FileId}");
                }
                Console.WriteLine();
            }
        }
    }   

    private static async Task RunInteractiveSessionAsync( PersistentAgentsClient agentsClient, PersistentAgent agent, PersistentAgentThread thread)
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
                await RunAgentConversation(userInput, agentsClient, agent, thread);
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