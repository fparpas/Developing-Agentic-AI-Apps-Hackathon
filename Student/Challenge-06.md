# Challenge 06 - Build your first Agent with Microsoft Agent Framework and integrate with MCP remote server

 [< Previous Challenge](./Challenge-04.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-06.md)
 
## Introduction

In this challenge, you will build your first intelligent application using **Microsoft Agent Framework**, Microsoft's open-source engine for developing agentic AI applications. You'll create an interactive console application that demonstrates the core capabilities of AI agent orchestration and tool integration.

Microsoft Agent Framework is the evolution of both Semantic Kernel and Autogen, combining the strengths of each into a unified platform. It takes the best features from Semantic Kernel—such as prompt engineering, plugin integration, and middleware orchestration—and merges them with Autogen's advanced agent collaboration and planning capabilities. This results in a powerful, flexible framework for building, deploying, and managing AI agents at scale.

By the end of this challenge, you'll have hands-on experience with agent creation, tool integration, and connecting external services through the Model Context Protocol (MCP). Microsoft Agent Framework enables you to build sophisticated AI agents that can reason, plan, and execute complex tasks, with built-in support for multi-agent collaboration, persistent memory, and seamless integration with various AI models and external services.

## Concepts

Before diving into the implementation, let's understand the key concepts that make Microsoft Agent Framework powerful for AI development.

### Microsoft Agent Framework Architecture

Microsoft Agent Framework provides a structured approach to AI agent development:

- **Agent Runtime**: The central orchestration engine that manages AI services, tools, and agent execution
- **AI Services**: Integration points with AI models (OpenAI, Azure OpenAI, etc.)
- **Tools**: Reusable components that extend the agent's capabilities
- **Function Calling**: The mechanism that allows AI models to execute your code
- **Planning**: Automatic orchestration of multiple tool calls to complete complex tasks
- **Agent State Management**: Persistent memory and context across conversations

### Tools and Function Calling

Tools are the building blocks of Microsoft Agent Framework applications:

- **Agent Tools**: Individual functions that can be called by the AI agent
- **Tool Descriptions**: Metadata that helps the AI understand when and how to use tools
- **Parameters**: Strongly-typed inputs and outputs for reliable tool execution
- **Auto Tool Calling**: AI agents automatically decide which tools to call based on user intent

### Integration with Model Context Protocol (MCP)

Microsoft Agent Framework can integrate with MCP servers to extend functionality:

- **MCP Client Integration**: Connect to remote MCP servers as additional capability sources
- **Tool Registration**: Convert MCP tools into Agent Framework tools
- **Hybrid Architecture**: Combine local tools with remote MCP services
- **Scalable Design**: Leverage both local processing and cloud-based services

### Agent Support and Framework

Microsoft Agent Framework provides comprehensive support for building intelligent agents:

- **Agent Framework**: Built-in abstractions for creating conversational AI agents with persistent state
- **Multi-Agent Systems**: Support for orchestrating multiple specialized agents working together
- **Agent Memory**: Persistent conversation history and context management across sessions
- **Agent Personas**: Define agent behavior, personality, and specialized knowledge domains
- **Collaborative Agents**: Enable agents to delegate tasks and share information with other agents
- **Agent Lifecycle Management**: Handle agent initialization, state management, and cleanup operations

## Description

This challenge will guide you through the process of developing your first intelligent app with Microsoft Agent Framework.

In just a few steps, you can build your first AI agent with Microsoft Agent Framework in either .NET.

### Task 1: Current time tool

In this task, you will create a tool that allows the AI agent to display the current time. Since large language models (LLMs) are trained on past data and do not have real-time capabilities, they cannot provide the current time on their own.

By creating this tool, you will enable the AI agent to call a function that retrieves and displays the current time.

#### Create a Current Time Tool

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

#### Create an agent and register the Current Time Tool

// Register the tool with your agent (e.g., during agent initialization)
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new ApiKeyCredential(apiKey))
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: instructions,
        name: agentName,
        tools: [AIFunctionFactory.Create(TimeTools.GetCurrentTimeInUTC)]
    );
```

Now, when you interact with your agent, you can ask for the current time and the agent will call this tool to provide an accurate response.

### Task 2: Integrate with Agent Service 

In this task, you will integrate the Agent Service into your Microsoft Agent Framework application created in previous challenge. This will allow your agent to leverage the capabilities of the Agent Service and check for travel policy compliance.

To integrate with the Agent Service, you will need to set up the `PersistentAgentsClient` and retrieve the agent using its ID.

```csharp
 var persistentAgentsClient = new PersistentAgentsClient(agentServiceEndpoint, new DefaultAzureCredential());

        // Retrieve the agent that was just created as an AIAgent using its ID
        AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentServiceId);
```

### Task 3: Integrate with Weather Remote MCP server

In this task you will integrate the Weather MCP Remote server completed in the previous challenge and add it as tools in Microsoft Agent Framework.

Initialize the MCP client with the following code:

```csharp
var mcpServerUrl = "Your remote MCP server endpoint";

_mcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(
        new SseClientTransportOptions
        {
            Endpoint = new Uri(mcpServerUrl),
            ConnectionTimeout = TimeSpan.FromMinutes(5) // Increase MCP connection timeout to 5 minutes
        }
    )
);
```

After creating the MCP client, you will get the list of tools and add them to Microsoft Agent Framework:

```csharp
var mcpTools = await _mcpClient.ListToolsAsync();

//List available MCP tools
Console.WriteLine("Available MCP Tools:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"- {tool.Name}: {tool.Description}");
}

//Register MCP tools to agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new ApiKeyCredential(apiKey))
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: instructions,
        name: agentName,
        tools: [.. mcpTools.Cast<AITool>().ToList()]
    );
```

## Success Criteria

- ✅ Ensure that your application is running and you are able to debug the application.
- ✅ Ensure that you are able to request the current time and receive an accurate response.
- ✅ Ensure that you are able to validate policy compliance functionality by ensuring the agent accurately answers travel policy questions
- ✅ Set a break point in one of the tools and hit the break point with a user prompt
- ✅ Debug and inspect the AgentThread object to see the sequence of tool calls and results.
- ✅ Integrate with MCP Remote server and get weather results.
- ✅ Demonstrate that the user can ask questions about weather data through the integrated MCP server.

## Learning Resources
- [Learn Microsoft Agent Framework in 3 minutes!](https://www.youtube.com/watch?v=Q881t44hWng)
- [Introducing Microsoft Agent Framework: The Open-Source Engine for Agentic AI Apps](https://devblogs.microsoft.com/foundry/introducing-microsoft-agent-framework-the-open-source-engine-for-agentic-ai-apps/)
- [Microsoft Agent Framework | MS Learn](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Microsoft Agent Framework | GitHub Repository](https://github.com/microsoft/agent-framework)
- [Microsoft Agent Framework .NET Samples | GitHub Repository](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples)
- [Microsoft Agent Framework Python Samples | GitHub Repository](https://github.com/microsoft/agent-framework/tree/main/python/samples)
- [Microsoft Agent Framework MCP Integration Guide](https://learn.microsoft.com/en-us/agent-framework/concepts/tools/adding-mcp-tools?pivots=programming-language-csharp)
