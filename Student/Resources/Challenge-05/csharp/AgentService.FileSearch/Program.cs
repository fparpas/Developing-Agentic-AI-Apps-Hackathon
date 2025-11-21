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
            var agentServiceEndpoint = configuration["AIAgentService:AiFFoundryEndpoint"];
            var agentServiceId = configuration["AIAgentService:AgentId"];

            Console.WriteLine("🔍 Azure Agent Service File Search Console Application");
            Console.WriteLine("===============================================");

            //TASK: Add your code here to run the agent conversation
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while running the application: {ex.Message}");
        }
    }

}