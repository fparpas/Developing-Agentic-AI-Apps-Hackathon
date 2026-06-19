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

        var searchCredential = new DefaultAzureCredential();

        // Define fields for the index
        var fields = new List<SearchField>
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SearchField("page_chunk", SearchFieldDataType.String) { IsFilterable = false, IsSortable = false, IsFacetable = false },
                new SearchField("page_embedding_text_3_large", SearchFieldDataType.Collection(SearchFieldDataType.Single)) { VectorSearchDimensions = 3072, VectorSearchProfileName = "hnsw_text_3_large" },
                new SimpleField("page_number", SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true, IsFacetable = true }
            };

        // Define a vectorizer
        var vectorizer = new AzureOpenAIVectorizer(vectorizerName: "azure_openai_text_3_large")
        {
            Parameters = new AzureOpenAIVectorizerParameters
            {
                ResourceUri = new Uri(aoaiEndpoint),
                DeploymentName = aoaiEmbeddingDeployment,
                ModelName = aoaiEmbeddingModel
            }
        };

        // Define a vector search profile and algorithm
        var vectorSearch = new VectorSearch()
        {
            Profiles =
                {
                    new VectorSearchProfile(
                        name: "hnsw_text_3_large",
                        algorithmConfigurationName: "alg"
                    )
                    {
                        VectorizerName = "azure_openai_text_3_large"
                    }
                },
            Algorithms =
                {
                    new HnswAlgorithmConfiguration(name: "alg")
                },
            Vectorizers =
                {
                    vectorizer
                }
        };

        // Define a semantic configuration
        var semanticConfig = new SemanticConfiguration(
            name: "semantic_config",
            prioritizedFields: new SemanticPrioritizedFields
            {
                ContentFields = { new SemanticField("page_chunk") }
            }
        );

        var semanticSearch = new SemanticSearch()
        {
            DefaultConfigurationName = "semantic_config",
            Configurations = { semanticConfig }
        };

        // Create the index
        var index = new SearchIndex(indexName)
        {
            Fields = fields,
            VectorSearch = vectorSearch,
            SemanticSearch = semanticSearch
        };

        // Create the index client, deleting and recreating the index if it exists
        var indexClient = new SearchIndexClient(new Uri(searchEndpoint), searchCredential);
        await indexClient.CreateOrUpdateIndexAsync(index);
        Console.WriteLine($"Index '{indexName}' created or updated successfully.");

        // Upload sample documents from the specified  URL
        string url = indexDataContentUrl;
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var documents = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        var searchClient = new SearchClient(new Uri(searchEndpoint), indexName, searchCredential);
        var searchIndexingBufferedSender = new SearchIndexingBufferedSender<Dictionary<string, object>>(
            searchClient,
            new SearchIndexingBufferedSenderOptions<Dictionary<string, object>>
            {
                KeyFieldAccessor = doc => doc["id"].ToString(),
            }
        );
        await searchIndexingBufferedSender.UploadDocumentsAsync(documents);
        await searchIndexingBufferedSender.FlushAsync();
        Console.WriteLine($"Documents uploaded to index '{indexName}' successfully.");

        // Create a knowledge source
        var indexKnowledgeSource = new SearchIndexKnowledgeSource(
            name: knowledgeSourceName,
            searchIndexParameters: new SearchIndexKnowledgeSourceParameters(searchIndexName: indexName)
            {
                SourceDataSelect = "id,page_chunk,page_number"
            }
        );
        await indexClient.CreateOrUpdateKnowledgeSourceAsync(indexKnowledgeSource);
        Console.WriteLine($"Knowledge source '{knowledgeSourceName}' created or updated successfully.");

        // Create a knowledge agent
        var openAiParameters = new AzureOpenAIVectorizerParameters
        {
            ResourceUri = new Uri(aoaiEndpoint),
            DeploymentName = aoaiGptDeployment,
            ModelName = aoaiGptModel
        };

        var agentModel = new KnowledgeAgentAzureOpenAIModel(azureOpenAIParameters: openAiParameters);
        var outputConfig = new KnowledgeAgentOutputConfiguration
        {
            Modality = KnowledgeAgentOutputConfigurationModality.AnswerSynthesis,
            IncludeActivity = true
        };

        var agent = new KnowledgeAgent(
            name: knowledgeAgentName,
            models: new[] { agentModel },
            knowledgeSources: new KnowledgeSourceReference[] {
                new KnowledgeSourceReference(knowledgeSourceName) {
                        IncludeReferences = true,
                        IncludeReferenceSourceData = true,
                        RerankerThreshold = (float?)2.5
                    }
            }
        )

        {
            OutputConfiguration = outputConfig
        };

        await indexClient.CreateOrUpdateKnowledgeAgentAsync(agent);
        Console.WriteLine($"Knowledge agent '{knowledgeAgentName}' created or updated successfully.");
        
        return indexClient;
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
                messages.Add(new Dictionary<string, string>
            {
                { "role", "user" },
                { "content", userInput }
            });

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

                messages.Add(new Dictionary<string, string>
            {
                { "role", "assistant" },
                { "content", (retrievalResult.Value.Response[0].Content[0] as KnowledgeAgentMessageTextContent).Text }
            });
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

    private static void ReviewResponseActivityAndResults(Azure.Response<KnowledgeAgentRetrievalResponse> retrievalResult)
    {
        // Print the response, activity, and results
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Response:");
        Console.WriteLine((retrievalResult.Value.Response[0].Content[0] as KnowledgeAgentMessageTextContent).Text);
        Console.ResetColor();

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

