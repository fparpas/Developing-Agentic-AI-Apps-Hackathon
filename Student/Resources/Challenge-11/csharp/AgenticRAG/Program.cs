using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents.Indexes.Models;
using System.Text.Json;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Agents;
using Azure.Identity;
using Azure.Search.Documents.Agents.Models;

#pragma warning disable SKEXP0010

class Program
{
    private static IConfiguration? _configuration;
    private static KnowledgeAgentRetrievalClient? _agentClient;
    static async Task Main(string[] args)
    {
        var agentInstructions = @"A Q&A agent that can answer questions about the Earth at night. If you don't have the answer, respond with ""I don't know"".";
        var indexDataContentUrl = "https://raw.githubusercontent.com/Azure-Samples/azure-search-sample-data/refs/heads/main/nasa-e-book/earth-at-night-json/documents.json";

        // Load configuration
        _configuration = LoadConfiguration();

        //Register Agentic Search Tool
        //Implement code in the method below
        SearchIndexClient indexClient = await RegisterAgenticSearch(indexDataContentUrl);

        // Start an interactive chat session
        await StartInteractiveChat(agentInstructions);

        //CleanUp Resources
        await CleanUpResources(indexClient);
    }

    static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static async Task<SearchIndexClient> RegisterAgenticSearch(string indexDataContentUrl)
    {
        // Load configuration settings
        var aoaiEndpoint = _configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Azure OpenAI endpoint is required");
        var aoaiKey = _configuration["AzureOpenAI:ApiKey"] ?? throw new InvalidOperationException("Azure OpenAI API key is required");
        var aoaiGptModel = _configuration["AzureOpenAI:Model"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");
        var aoaiGptDeployment = _configuration["AzureOpenAI:DeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI deployment name is required");
        var aoaiEmbeddingModel = _configuration["AzureOpenAI:EmbeddingsModel"] ?? throw new InvalidOperationException("Azure OpenAI embeddings deployment name is required");
        var aoaiEmbeddingDeployment = _configuration["AzureOpenAI:EmbeddingsDeploymentName"] ?? throw new InvalidOperationException("Azure OpenAI embeddings deployment name is required");

        var searchEndpoint = _configuration["AzureAISearch:Endpoint"] ?? throw new InvalidOperationException("Azure Search AI endpoint is required");
        var searchKey = _configuration["AzureAISearch:SearchKey"] ?? throw new InvalidOperationException("Azure Search AI key is required");
        var indexName = _configuration["AzureAISearch:IndexName"] ?? throw new InvalidOperationException("Azure Search AI index name is required");
        var knowledgeSourceName = _configuration["AzureAISearch:KnowledgeSourceName"] ?? throw new InvalidOperationException("Azure Search AI knowledge source name is required");
        var knowledgeAgentName = _configuration["AzureAISearch:KnowledgeAgentName"] ?? throw new InvalidOperationException("Azure Search AI knowledge agent name is required");

        // Create a credential using DefaultAzureCredential
        var credential = new DefaultAzureCredential();

        // Add your code here tom complete this challenge
        // - Create a SearchIndexClient
        // - Upload data to the index
        // - Create a knowledge source
        // - Create a knowledge agent        
        
        return null; // Replace with actual SearchIndexClient instance
    }

    private static async Task CleanUpResources(SearchIndexClient indexClient)
    {
        var indexName = _configuration["AzureAISearch:IndexName"] ?? throw new InvalidOperationException("Azure Search AI index name is required");
        var knowledgeSourceName = _configuration["AzureAISearch:KnowledgeSourceName"] ?? throw new InvalidOperationException("Azure Search AI knowledge source name is required");
        var knowledgeAgentName = _configuration["AzureAISearch:KnowledgeAgentName"] ?? throw new InvalidOperationException("Azure Search AI knowledge agent name is required");


        // Clean up resources
        await indexClient.DeleteKnowledgeAgentAsync(knowledgeAgentName);
        Console.WriteLine($"Knowledge agent '{knowledgeAgentName}' deleted successfully.");

        await indexClient.DeleteKnowledgeSourceAsync(knowledgeSourceName);
        Console.WriteLine($"Knowledge source '{knowledgeSourceName}' deleted successfully.");

        await indexClient.DeleteIndexAsync(indexName);
        Console.WriteLine($"Index '{indexName}' deleted successfully.");
    }

    private static async Task StartInteractiveChat(string instructions)
    {
        var searchEndpoint = _configuration["AzureAISearch:Endpoint"] ?? throw new InvalidOperationException("Azure Search AI endpoint is required");
       var knowledgeAgentName = _configuration["AzureAISearch:KnowledgeAgentName"] ?? throw new InvalidOperationException("Azure Search AI knowledge agent name is required");
        

        Console.WriteLine("Agentic Search Chat");
        Console.WriteLine("Type 'exit' to quit.\n");

        //Set system message instructions
        var messages = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    { "role", "system" },
                    { "content", instructions }
                }
            };

            // Use agentic retrieval to fetch results
            var agentClient = new KnowledgeAgentRetrievalClient(
                endpoint: new Uri(searchEndpoint),
                agentName: knowledgeAgentName,
                tokenCredential: new DefaultAzureCredential()
            );           

        // Start chat loop
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
            // Add user message to the conversation
            try
            {
                messages.Add(new Dictionary<string, string>
            {
                { "role", "user" },
                { "content", userInput }
            });
                // Call the agentic retrieval client
                var retrievalResult = await agentClient.RetrieveAsync(
                    retrievalRequest: new KnowledgeAgentRetrievalRequest(
                        messages: messages
                            .Where(message => message["role"] != "system")
                            .Select(
                                message => new KnowledgeAgentMessage(content: new[] { new KnowledgeAgentMessageTextContent(message["content"]) }) { Role = message["role"] }
                            )
                            .ToList()
                    )
                );
                // Add assistant response to the conversation
                messages.Add(new Dictionary<string, string>
            {
                { "role", "assistant" },
                { "content", (retrievalResult.Value.Response[0].Content[0] as KnowledgeAgentMessageTextContent).Text }
            });
                // Review the response, activity, and results
                ReviewResponseActivityAndResults(retrievalResult);
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

    // Method to review and print the response, activity, and results
    private static void ReviewResponseActivityAndResults(Azure.Response<KnowledgeAgentRetrievalResponse> retrievalResult)
    {
        // Print the response
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Response:");
        Console.WriteLine((retrievalResult.Value.Response[0].Content[0] as KnowledgeAgentMessageTextContent).Text);
        Console.ResetColor();

        // Print the activity
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Activity:");
        foreach (var activity in retrievalResult.Value.Activity)
        {
            Console.WriteLine($"Activity Type: {activity.GetType().Name}");
            string activityJson = JsonSerializer.Serialize(
                activity,
                activity.GetType(),
                new JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine(activityJson);
        }
        Console.ResetColor();

        // Print the results
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Results:");
        foreach (var reference in retrievalResult.Value.References)
        {
            Console.WriteLine($"Reference Type: {reference.GetType().Name}");
            string referenceJson = JsonSerializer.Serialize(
                reference,
                reference.GetType(),
                new JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine(referenceJson);
        }
        Console.ResetColor();
    }
}

