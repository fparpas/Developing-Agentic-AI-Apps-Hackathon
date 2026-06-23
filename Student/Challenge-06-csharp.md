# Challenge 06 - C# - Build your first Agent with Microsoft Agent Framework and integrate with MCP remote server

 [< Previous Challenge](./Challenge-05-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-07-csharp.md)
 
[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-06-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-06-python.md)

## Introduction

In this challenge, you will build your first intelligent application using **Microsoft Agent Framework**, Microsoft's open-source engine for developing agentic AI applications. You'll create an interactive console application that demonstrates the core capabilities of AI agent orchestration and tool integration.

Microsoft Agent Framework is the evolution of both Semantic Kernel and Autogen, combining the strengths of each into a unified platform. It takes the best features from Semantic Kernel, such as prompt engineering, plugin integration, and middleware orchestration—and merges them with Autogen's advanced agent collaboration and planning capabilities. This results in a powerful, flexible framework for building, deploying, and managing AI agents at scale.

By the end of this challenge, you'll have hands-on experience with agent creation, tool integration, and connecting external services through the Model Context Protocol (MCP). Microsoft Agent Framework enables you to build sophisticated AI agents that can reason, plan, and execute complex tasks, with built-in support for multi-agent collaboration, persistent memory, and seamless integration with various AI models and external services.

## Concepts

Before diving into the implementation, let's understand the key concepts that make Microsoft Agent Framework powerful for AI development.

### Agent Framework offers two primary categories of capabilities:

#### Agents

Individual agents that use LLMs to process inputs, call tools and MCP servers, and generate responses. Supports Azure OpenAI, OpenAI, Anthropic, Ollama, and more.

#### Workflows	

Graph-based workflows that connect agents and functions for multi-step tasks with type-safe routing, checkpointing, and human-in-the-loop support.

### Microsoft Agent Framework Architecture

Microsoft Agent Framework provides a structured approach to AI agent development:

- **Agent Runtime**: The central orchestration engine that manages AI services, tools, and agent execution

- **AI Services**: Integration points with AI models (OpenAI, Azure OpenAI, etc.)

- **Tools**: Reusable components that extend the agent's capabilities

- **Function Calling**: The mechanism that allows AI models to decide and enable the execution of your code

- **Planning**: Automatic orchestration of multiple tool calls to complete complex tasks

- **Agent State Management**: Persistent memory and context across conversations

### Microsoft Agent Framework core components

The Microsoft Agent Framework offers different components that can be used individually or combined.

- **Chat clients** - provide abstractions for connecting to AI services from different providers under a common interface. Any inference service that provides a `Microsoft.Extensions.AI.IChatClient` implementation can be used to build an agent via the `ChatClientAgent` class. Supported providers include Azure OpenAI, OpenAI, Anthropic, and more.

- **Function tools** - containers for custom functions that extend agent capabilities. Agents can automatically invoke functions with your own logic and integrate also with MCP servers and services.

- **Built-in tools** - prebuilt capabilities including Code Interpreter for Python execution, File Search for document analysis, and Web Search for internet access.

- **Conversation management** - structured message system with roles (USER, ASSISTANT, SYSTEM, TOOL) and AgentSession for persistent conversation context across interactions.

- **Workflow orchestration** - supports sequential workflows, concurrent execution, handoff and Magentic patterns for complex multi-agent collaboration.

### Microsoft Agent Framework Agent Types
The Microsoft Agent Framework provides support for several types of agents to accommodate different use cases and requirements.

All agents are derived from a common base class, AIAgent, which provides a consistent interface for all agent types. This allows for building common, agent agnostic, higher level functionality such as multi-agent orchestrations.

| Agent Type                  | Description                                                        | Service Chat History storage supported | InMemory/Custom Chat History storage supported |
|-----------------------------|--------------------------------------------------------------------|----------------------------------------|---------------------------------------|
| Microsoft Foundry Agent     | An agent that uses the Foundry Agent Service as its backend.        | Yes                                    | No                                    |
| Foundry Models ChatCompletion | An agent that uses any of the models deployed in the Foundry Service as its backend via ChatCompletion. | No                                     | Yes                                   |
| Foundry Models Responses    | An agent that uses any of the models deployed in the Foundry Service as its backend via Responses. | Yes                                    | Yes                                   |
| Foundry Anthropic           | An agent that uses a Claude model via the Foundry Anthropic Service as its backend. | No                                     | Yes                                   |
| Azure OpenAI ChatCompletion | An agent that uses the Azure OpenAI ChatCompletion service.         | No                                     | Yes                                   |
| Azure OpenAI Responses      | An agent that uses the Azure OpenAI Responses service.              | Yes                                    | Yes                                   |
| Anthropic                   | An agent that uses a Claude model via the Anthropic Service as its backend. | No                                     | Yes                                   |
| OpenAI ChatCompletion       | An agent that uses the OpenAI ChatCompletion service.               | No                                     | Yes                                   |
| OpenAI Responses            | An agent that uses the OpenAI Responses service.                    | Yes                                    | Yes                                   |
| Any other `IChatClient`     | You can also use any other Microsoft.Extensions.AI.IChatClient implementation to create an agent. | Varies                                 | Varies                                |

### Integration with Model Context Protocol (MCP)

Microsoft Agent Framework can integrate with MCP servers to extend functionality:

- **MCP Client Integration**: Connect to remote MCP servers as additional capability sources
- **Tool Registration**: Convert MCP tools into Agent Framework tools
- **Hybrid Architecture**: Combine local tools with remote MCP services
- **Scalable Design**: Leverage both local processing and cloud-based services

## Description

This challenge will guide you through the process of developing your first intelligent app with Microsoft Agent Framework.

To help you get started, a starter project is available at `Student/Resources/Challenge-06/csharp`. Use this as your foundation and implement the tasks outlined below by completing the relevant sections of the starter project.

### Task 1: Current time tool

In this task, you will create an agent and a tool that lets the agent display the current time. Since large language models (LLMs) are trained on past data and do not have real-time capabilities, they cannot provide the current time on their own.

By creating this function tool, you will enable the AI agent to call a function that retrieves and displays the current time.

#### Create a time agent that has Current Time function tool

Add a method to retrieve the current time and register it as a tool in your agent.

```csharp
public static class TimeTools
{
    [Description("Returns the current system time in UTC.")]
    public static string GetCurrentTimeInUTC()
    {
        return $"The current time in UTC is {DateTime.UtcNow}";
    }
}
```

#### Create an agent and register the Current Time Tool

```csharp
// Register the tool with your agent (e.g., during agent initialization)
AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: instructions,
                name: agentName,
                tools: [AIFunctionFactory.Create(TimeTools.GetCurrentTimeInUTC)]
            );
```

#### Run the agent

```csharp
AgentSession session = await agent.CreateSessionAsync();
var response = await agent.RunAsync("What time is it now?", session);
Console.WriteLine(response);
```

Now, when you interact with your agent, you can ask for the current time and the agent will call this tool to provide an accurate response.

### Task 2: Integrate with Weather Remote MCP server

In this task you will create a weather agent in MAF and integrate the Weather MCP Remote server completed in the previous challenge and add it as tools in Microsoft Agent Framework.

Initialize the MCP client with the following code:

```csharp
var mcpServerUrl = "Your remote MCP server endpoint";

var mcpClient = await McpClient.CreateAsync(
           new HttpClientTransport(
               new HttpClientTransportOptions()
               {
                   Endpoint = new Uri(mcpServerUrl)
               }
           )
        );
```

After creating the MCP client, you will get the list of tools and add them to Microsoft Agent Framework:

```csharp
//Get list of tools from MCP server
var mcpTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"Found {mcpTools.Count} MCP tools");

Console.WriteLine("Available MCP Tools:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"- {tool.Name}: {tool.Description}");
}

// Create an agent and register MCP tools with the agent
 AIAgent agent = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .AsAIAgent(
                instructions: instructions,
                name: agentName,
                tools: [.. mcpTools.Cast<AITool>().ToList()]
            );
```

#### Run the agent

```csharp
AgentSession session = await agent.CreateSessionAsync();
var response = await agent.RunAsync("What is the weather in New York?", session);
Console.WriteLine(response);
```

### Task 3: Integrate with Azure AI Foundry Agents Service

In this task, you will integrate the Agent Service into your Microsoft Agent Framework application created in previous challenge. This will allow your agent to leverage the capabilities of the Microsoft Foundry Agent Service and check for travel policy compliance.

To integrate with the Agent Service, you will need to set up the `AIProjectClient` and retrieve the agent using its name.

```csharp
AIProjectClient projectClient = new(endpoint: new Uri(agentServiceEndpoint), tokenProvider: new DefaultAzureCredential());

AIAgent aiAgent = await projectClient.GetAIAgentAsync(agentName);
```

#### Run the agent

```csharp
AgentSession session = await aiAgent.CreateSessionAsync();
var response = await aiAgent.RunAsync("What is the maximum daily meal allowance for domestic travel?", session);
Console.WriteLine(response);
```

## Success Criteria

- ✅ Ensure that your application is running and you are able to debug the application.
- ✅ Ensure that you are able to request the current time and receive an accurate response.
- ✅ Ensure that you are able to validate policy compliance functionality by ensuring the agent accurately answers travel policy questions
- ✅ Set a break point in one of the tools and hit the break point with a user prompt
- ✅ Debug and inspect the AgentSession object to see the sequence of tool calls and results.
- ✅ Integrate with MCP Remote server and get weather results.
- ✅ Demonstrate that the user can ask questions about weather data through the integrated MCP server.

## Learning Resources
- [Microsoft Agent Framework | MS Learn](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Microsoft Agent Framework | GitHub Repository](https://github.com/microsoft/agent-framework)
- [Microsoft Agent Framework .NET Samples | GitHub Repository](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples)
- [Microsoft Agent Framework Python Samples | GitHub Repository](https://github.com/microsoft/agent-framework/tree/main/python/samples)
