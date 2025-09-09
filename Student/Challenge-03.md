# Challenge 03 - Build your first MCP client

 [< Previous Challenge](./Challenge-02.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-04.md)

## Introduction

In this challenge, you will build your first Model Context Protocol (MCP) client using C# and .NET. While in the previous challenge you created an MCP server that provides tools, now you'll create a client that can connect to MCP servers, discover their capabilities, and interact with their tools through an AI assistant.

## Concepts

An MCP client is responsible for:
- **Connecting to MCP servers**: Establishing communication with one or more MCP servers via standard transport (typically stdio)
- **Service discovery**: Listing available tools, resources, and prompts from connected servers
- **Tool orchestration**: Calling tools on behalf of an AI assistant and handling responses
- **Session management**: Managing the lifecycle of connections and conversations

The typical flow is:
1. Client connects to one or more MCP servers
2. Client discovers available tools from each server
3. User makes a query to the client
4. Client forwards the query and available tools to an AI assistant
5. AI assistant decides which tools to use and makes tool calls
6. Client executes tool calls on the appropriate servers
7. Client returns results to the AI assistant
8. AI assistant provides a natural language response to the user

## Description

In this challenge, you will build a console-based MCP client that can connect to your Weather MCP Server from the previous challenge (or any other MCP server) and provide an interactive chat interface where users can ask questions that leverage the server's tools.

### Task 1: Set up your environment

Create a new .NET console application for the MCP client:

```bash
dotnet new console -n WeatherMcpClient
cd WeatherMcpClient
```

Add the required NuGet packages:
```bash
# Add the Model Context Protocol SDK
dotnet add package ModelContextProtocol.Client --prerelease
# Add Azure OpenAI client
dotnet add package Azure.AI.OpenAI --prerelease
```

### Task 2: Configure Azure OpenAI and get environment variables

**Prerequisites:**
1. Create an Azure OpenAI resource in the Azure portal
2. Deploy a GPT-4 model in your Azure OpenAI resource
3. Note down your endpoint URL, API key, and deployment name

**Note:** You can find these values in your Azure OpenAI resource:
- **Endpoint**: In the "Keys and Endpoint" section of your Azure OpenAI resource
- **API Key**: Also in the "Keys and Endpoint" section
- **Deployment Name**: The name you gave when deploying your GPT model in Azure OpenAI Studio

Add environment variable setup for Azure OpenAI. Create a `.env` file in your project:

```env
AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_OPENAI_API_KEY=your_azure_openai_api_key_here
AZURE_OPENAI_DEPLOYMENT_NAME=your_gpt4_deployment_name
```

Replace the contents of `Program.cs` to get environment variables

```csharp
using ModelContextProtocol.Client;
using Azure.AI.OpenAI;
using Azure;

// Initialize Azure OpenAI client
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "<Add your endpoint>";
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "<Add your API key>";
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4";

if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Please set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY environment variables.");
    return;
}
```

### Task 3: Setup MCP client

Setup the MCP Client. This will create an MCP client that will connect to a server that is provided as a command line argument. It then lists the available tools from the connected server.

```csharp
var clientTransport = new StdioClientTransport(new()
{
    Name = "Weather MCP Server",
    Command = "dotnet",
    Arguments = ["run", "--project", "../WeatherMcpServer/WeatherMcpServer.csproj"]
});

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

### Task 3: Add Azure OpenAI Integration

Implement the Azure OpenAI integration by adding the following code

```csharp

var openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

var chatCompletionsOptions = new ChatCompletionsOptions(deploymentName, new[]
    {
        new ChatRequestUserMessage(query)
    })
    {
        MaxTokens = 1000,
        Temperature = 0.7f,
        Tools = ConvertToAzureOpenAITool(tools)
    };
```

This method will convert MCP tools to Azure OpenAI tools.

```csharp
ChatCompletionsFunctionToolDefinition ConvertToAzureOpenAITool(Tool mcpTool)
{
    return new ChatCompletionsFunctionToolDefinition()
    {
        Name = mcpTool.Name,
        Description = mcpTool.Description,
        Parameters = BinaryData.FromString(JsonSerializer.Serialize(mcpTool.InputSchema))
    };
}
```

### Task 4: Query processing logic

Add the core functionality for processing queries and handling tool calls:

```csharp

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("MCP Client Started!");
Console.ResetColor();

PromptForInput();
while(Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(query))
    {
        PromptForInput();
        continue;
    }

    var response = await openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);

    Console.Write(response.Value.Choices[0].Message.Content);
    
    Console.WriteLine();

    PromptForInput();
}

static void PromptForInput()
{
    Console.WriteLine("Enter a command (or 'exit' to quit):");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("> ");
    Console.ResetColor();
}
```

### Task 5: Test with your Weather Server

Run your Weather MCP Server from the previous challengeand test the client:

```bash
# In one terminal, ensure your weather server works:
cd WeatherMcpServer
dotnet run

# In another terminal, run your client:
cd WeatherMcpClient
dotnet run dotnet run --project ../WeatherMcpServer
```

Try queries like:
- "What's the weather in Sacramento?"
- "Are there any weather alerts for California?"
- "Give me the forecast for New York City"

## Success Criteria

- A .NET MCP client application that can connect to MCP servers over stdio
- Client can discover and list tools from connected servers
- Interactive console interface for user queries
- Integration with Azure OpenAI for natural language processing
- AI assistant can decide which tools to use based on user queries
- Client can execute tool calls on MCP servers and return results to the AI
- User can ask "What's the weather in Sacramento?" and get a natural language response
- Proper error handling and resource cleanup
- Client works with the Weather MCP Server from Challenge 02

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [MCP Client Quickstart](https://modelcontextprotocol.io/quickstart/client)
- [C# Client Implementation Guide](https://modelcontextprotocol.io/quickstart/client#c%23)
- [MCP SDK Documentation](https://modelcontextprotocol.io/docs/sdk)
- [Azure OpenAI Service Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Azure OpenAI .NET SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/openai/Azure.AI.OpenAI)
- [.NET Hosting Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [MCP Architecture](https://modelcontextprotocol.io/legacy/concepts/architecture)