# Challenge 03 - C# - Build your first MCP client

 [< Previous Challenge](./Challenge-02-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-04-csharp.md)

[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-03-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-03-python.md)

## Introduction

In this challenge, you will build your first Model Context Protocol (MCP) client using .NET C#.  While in the previous challenge you created an MCP server that provides tools, now you'll create a client that can connect to MCP servers, discover their capabilities, and interact with their tools through an AI assistant.

## Concepts

An MCP client is a software application that connects to Model Context Protocol (MCP) servers to enable AI-powered interactions with external tools and services. It acts as a bridge between users, AI models, and MCP servers, handling tasks such as discovering available tools, orchestrating function calls, and managing communication sessions. By leveraging the MCP client, applications can dynamically invoke server-side functions in response to user queries, enabling richer and more context-aware AI experiences.

The MCP client is responsible for:
- **Connecting to MCP servers**: Establishing communication with one or more MCP servers via standard transport (typically stdio)
- **Service discovery**: Listing available tools, resources, and prompts from connected servers
- **Tool orchestration**: Calling tools on behalf of an AI assistant and handling responses
- **Session management**: Managing the lifecycle of connections and conversations

### MCP Function Calling 

### What is Function Calling?

Function calling is a mechanism that allows AI models to invoke external functions or tools in response to user queries. Instead of only generating text, the AI can recognize when a user's request requires real-world data or actions, and then call the appropriate function with the necessary parameters. This enables the AI to interact with external systems, retrieve information, or perform operations beyond its built-in knowledge.

Function calling is the enabler for AI reasoning, allowing the model to dynamically decide when to use its own knowledge and when to leverage external tools. By orchestrating function calls, the AI can provide more accurate, actionable, and context-aware responses, bridging the gap between conversational understanding and real-world execution.

### MCP Function Calling Flow
The MCP Function Calling Flow illustrates how an MCP client interacts with both the user and the MCP server to fulfill requests using AI and available tools. The process begins when a user submits a prompt to the MCP client. The client then communicates with the MCP server to retrieve the available tool schemas, which describe the functions and capabilities the server offers. These schemas are included in the request sent to the AI model, enabling it to understand which tools it can invoke to address the user's query.

The AI model analyzes the user's input, the conversation history, and the provided tool schemas. Based on this context, it determines whether to respond directly to the user or to invoke a specific tool by generating a function call. If a tool invocation is required, the MCP client extracts the necessary parameters from the model's response and instructs the MCP server to execute the function. The result is then relayed back to the user, completing the interaction. This flow enables seamless orchestration between user intent, AI reasoning, and tool execution.

![MCP Client Architecture](./Resources/Diagrams/FunctionCallingWithMCP.jpg)

#### Steps for MCP Function Calling Flow
1. Person makes a request
    - The user initiates an action or query.
2. MCP Client retrieves tool schema
   - The MCP Client requests the tools schema from the MCP Server and adds it to the model request.
3. MCP Server returns tools schema
   - The MCP Server responds with the tools schema to the MCP Client.
4. Request sent to the model
   - The MCP Client sends the user request, conversation history, tools schema, and any previous function results to the AI model.
5. Model generates a response
 - The model processes the request and generates a response.
 - The model decides whether to:
    - Provide a Chat Response, or
    - Provide a Function Response (tool invocation)
6. Handle response
   - If the response is a Chat Response:
     - The MCP Client sends the response back to the user.
    - If the response is a Function Response:
      - The MCP Client extracts tool function parameters from the model’s response.
7. Execute function and return result
    - MCP Server executes function using the provided parameters.
    - Return function result to the MCP Client and then to the user.

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
# Add Microsoft Extensions AI for OpenAI
dotnet add package Microsoft.Extensions.AI
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
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

Add environment variable to retrieve configuration values for your project:

```env
AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_OPENAI_API_KEY=your_azure_openai_api_key_here
AZURE_OPENAI_DEPLOYMENT_NAME=your_gpt4_deployment_name
```

> ℹ️ Storing configuration values like API endpoints in environment variables keeps them separate from your source code, making your application more flexible. For sensitive information such as API keys, it's best to use a secure storage solution.

```csharp

// Initialize Azure OpenAI client
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "<Add your endpoint>";
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "<Add your API key>";
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4";

```

### Task 3: Setup MCP client

Setup the MCP Client. This will create an MCP client that will connect to a server that is provided as a command line argument. It then lists the available tools from the connected server.

```csharp
var clientTransport = new StdioClientTransport(new()
{
    Name = "Weather MCP Server",
    Command = "dotnet",
    Arguments = ["run", "--project", "<add the weather MCP server project path>"]
});

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}
```

### Task 3: Add Azure OpenAI Integration

Implement the Azure OpenAI integration by adding the following code

```csharp
IChatClient client = new ChatClientBuilder(
    new AzureOpenAIClient(new Uri(endpoint),
    new ApiKeyCredential(apiKey))
    .GetChatClient(deploymentName).AsIChatClient())
    .UseFunctionInvocation()
    .Build();
```

### Task 4: Query processing logic

Add the core functionality for processing queries

```csharp
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

- ✅ A .NET MCP client application that can connect to MCP servers over stdio
- ✅ Client can discover and list tools from connected servers
- ✅ AI assistant can decide which tools to use based on user queries
- ✅ Client can execute tool calls on MCP servers and return results to the AI application
- ✅ User can ask "What's the weather in Sacramento?" and get a natural language response
- ✅ Client works with the Weather MCP Server from previous challenge

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [MCP Client Quickstart](https://modelcontextprotocol.io/quickstart/client)
- [C# Client Implementation Guide](https://modelcontextprotocol.io/quickstart/client#c%23)
- [MCP SDK Documentation](https://modelcontextprotocol.io/docs/sdk)
- [Azure OpenAI Service Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Azure OpenAI .NET SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/openai/Azure.AI.OpenAI)
- [MCP Architecture](https://modelcontextprotocol.io/legacy/concepts/architecture)
- [Deep Dive: Understanding MCP Client-Server Communication with Agent and LLM](https://medium.com/@jamestang/deep-dive-understanding-mcp-client-server-communication-with-agent-and-llm-aa4782a65991)
