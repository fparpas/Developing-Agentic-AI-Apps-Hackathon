# Challenge 06 - Python - Build your first Agent with Microsoft Agent Framework and integrate with MCP remote server

 [< Previous Challenge](./Challenge-05-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-07-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-06-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-06-python.md)

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

- **Function Calling**: The mechanism that allows AI models to decide and enable the execution of your code

- **Planning**: Automatic orchestration of multiple tool calls to complete complex tasks

- **Agent State Management**: Persistent memory and context across conversations

### Microsoft Agent Framework core components

The Microsoft Agent Framework offers different components that can be used individually or combined.

- **Chat clients** - provide abstractions for connecting to AI services from different providers under a common interface. Supported providers include Azure OpenAI, OpenAI, Anthropic, and more through the BaseChatClient abstraction.

- **Function tools** - containers for custom functions that extend agent capabilities. Agents can automatically invoke functions with your own logic and integrate also with MCP servers and services.

- **Built-in tools** - prebuilt capabilities including Code Interpreter for Python execution, File Search for document analysis, and Web Search for internet access.

- **Conversation management** - structured message system with roles (USER, ASSISTANT, SYSTEM, TOOL) and AgentThread for persistent conversation context across interactions.

- **Workflow orchestration** - supports sequential workflows, concurrent execution, handoff and Magentic patterns for complex multi-agent collaboration.

### Microsoft Agent Framework Agent Types
The Microsoft Agent Framework provides support for several types of agents to accommodate different use cases and requirements.

All agents are derived from a common base class, AIAgent, which provides a consistent interface for all agent types. This allows for building common, agent agnostic, higher level functionality such as multi-agent orchestrations.

| Agent Type                  | Description                                                        | Service Chat History storage supported | Custom Chat History storage supported |
|-----------------------------|--------------------------------------------------------------------|----------------------------------------|---------------------------------------|
| Azure AI Foundry Agent      | An agent that uses the Azure AI Foundry Agents Service as its backend. | Yes                                    | No                                    |
| Azure OpenAI ChatCompletion | An agent that uses the Azure OpenAI ChatCompletion service.         | No                                     | Yes                                   |
| Azure OpenAI Responses      | An agent that uses the Azure OpenAI Responses service.              | Yes                                    | Yes                                   |
| OpenAI ChatCompletion       | An agent that uses the OpenAI ChatCompletion service.               | No                                     | Yes                                   |
| OpenAI Responses            | An agent that uses the OpenAI Responses service.                    | Yes                                    | Yes                                   |
| OpenAI Assistants           | An agent that uses the OpenAI Assistants service.                   | Yes                                    | No                                    |
| Any other ChatClient        | Any class inheriting from BaseChatClient, like AzureOpenAIResponsesClient and AzureOpenAIChatCompletionClient | Varies                                 | Varies                                |

### Integration with Model Context Protocol (MCP)

Microsoft Agent Framework can integrate with MCP servers to extend functionality:

- **MCP Client Integration**: Connect to remote MCP servers as additional capability sources
- **Tool Registration**: Convert MCP tools into Agent Framework tools
- **Hybrid Architecture**: Combine local tools with remote MCP services
- **Scalable Design**: Leverage both local processing and cloud-based services

## Description

This challenge will guide you through the process of developing your first intelligent app with Microsoft Agent Framework.

In just a few steps, you can build your first AI agent with Microsoft Agent Framework in Python.

`requirements.txt`:

```plaintxt
azure-ai-agents>=1.2.0b5
agent-framework>=1.0.0b251114
mcp[cli]>=1.2.0
python-dotenv>=1.0.0
```

### Task 1: Current time tool

In this task, you will create a tool that allows the AI agent to display the current time. Since large language models (LLMs) are trained on past data and do not have real-time capabilities, they cannot provide the current time on their own.

By creating this tool, you will enable the AI agent to call a function that retrieves and displays the current time.

#### Create a Current Time Tool

Add a method to retrieve the current time and register it as a tool in your agent.

```python
from datetime import datetime
from agent_framework import Function, FunctionParameter, FunctionResult

def get_current_time_utc() -> str:
    """Returns the current system time in UTC."""
    return f"The current time in UTC is {datetime.utcnow().isoformat()}"

# Create a Function object to register with the agent
current_time_function = Function(
    name="get_current_time_utc",
    description="Returns the current system time in UTC",
    parameters=[],
    # handler expects FunctionResult type
    handler=lambda: FunctionResult(get_current_time_utc())
)
```

#### Create an agent and register the Current Time Tool

```python
from agent_framework import ChatAgent
from agent_framework.azure import AzureOpenAIResponsesClient
import os

# Consider storing secrets in an .env file and loading
# them using python-dotenv and load_dotenv()
# See https://pypi.org/project/python-dotenv/

async def create_agent_with_tools():
    """Create an agent with the current time tool registered."""

    # Create the chat client
    # AZURE_OPENAI_DEPLOYMENT_NAME should be the name of your model deployment on Microsoft Foundry/Azure OpenAI
    # i.e. "gpt-4o-mini"
    chat_client = AzureOpenAIResponsesClient(
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        api_key=os.getenv("AZURE_OPENAI_API_KEY"),
        deployment_name=os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
        api_version=os.getenv("AZURE_OPENAI_API_VERSION", "latest")
    )

    # Define the current time tool as a Function
    current_time_function = Function(
        name="get_current_time_utc",
        description="Returns the current system time in UTC",
        parameters=[],
        # handler expects FunctionResult type
        handler=lambda: FunctionResult(get_current_time_utc())
    )

    # Create the agent with the tool registered
    agent = ChatAgent(
        name="TimeAgent",
        client=chat_client,
        instructions="You are a helpful assistant. When the user asks for the current time, use the get_current_time_utc tool to provide an accurate response.",
        tools=[current_time_function]
    )

    return agent
```

#### Chat with your agent

To interact with your agent, add a main function that handles the conversation loop:

```python
async def main():
    """Main entry point for chatting with the agent."""
    agent = await create_agent_with_tools()

    print("Chat with TimeAgent (type 'exit' to quit)")
    print("-" * 50)

    while True:
        user_input = input("\nYou: ").strip()

        if user_input.lower() in ("exit", "quit"):
            print("Bu-bye!")
            break

        if not user_input:
            continue

        # Stream response from agent
        print("\nAgent: ", end="", flush=True)
        async for update in agent.run_stream(user_input):
            if update.text:
                print(update.text, end="", flush=True)
        print()

if __name__ == "__main__":
    import asyncio
    asyncio.run(main())
```

Now, when you run your script and interact with your agent, you can ask for the current time and the agent will call the tool to provide an accurate response.

**Example interaction:**
```
You: What's the current time?
Agent: The current time in UTC is 2025-11-27T14:30:45.123456
```

### Task 2: Integrate with Agent Service

In this task, you will integrate the Agent Service into your Microsoft Agent Framework application created in previous challenge. This will allow your agent to leverage the capabilities of the Agent Service and check for travel policy compliance.

To integrate with the Agent Service, you will need to set up the `ProjectsClient` and retrieve the agent using its ID.

```python
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
import os

async def get_agent_from_service():
    """Retrieve an agent from Azure AI Foundry Agent Service."""

    # Initialize the Azure AI Project client
    endpoint = os.getenv("AZURE_AI_PROJECT_ENDPOINT")
    project_client = AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())

    # Get the agents client
    agents_client = project_client.agents

    # Retrieve the agent by its ID
    agent_id = os.getenv("AGENT_ID")
    agent = agents_client.get_agent(agent_id)

    return agent, agents_client
```

### Task 3: Integrate with Weather Remote MCP server

In this task you will integrate the Weather MCP Remote server completed in the previous challenge and add it as tools in Microsoft Agent Framework.

Initialize the MCP client with the following code:

```python
from mcp import ClientSession
from mcp.client.sse import SseClientTransport
import os
import asyncio

async def create_mcp_client():
    """Create and connect to an MCP server via SSE transport."""

    # Define the MCP server endpoint
    mcp_server_url = os.getenv("MCP_SERVER_URL", "http://localhost:8000")

    # Create SSE transport for HTTP-based connection
    transport = SseClientTransport(mcp_server_url)

    # Create MCP client session
    async with ClientSession(transport) as session:
        # Initialize the connection
        await session.initialize()

        return session
```

After creating the MCP client, you will get the list of tools and add them to Microsoft Agent Framework:

```python
async def integrate_mcp_tools_with_agent(agent, mcp_session):
    """Integrate MCP tools with the AI agent."""

    # Get the list of available tools from the MCP server
    tools_response = await mcp_session.list_tools()
    mcp_tools = tools_response.tools if hasattr(tools_response, 'tools') else tools_response

    # Display available MCP tools
    print("Available MCP Tools:")
    for tool in mcp_tools:
        print(f"- {tool.name}: {tool.description}")

    # Convert MCP tools to Agent Framework Function objects
    agent_functions = []
    for mcp_tool in mcp_tools:
        async def tool_handler(tool=mcp_tool, session=mcp_session, **kwargs):
            """Wrapper to call MCP tool through the session."""
            result = await session.call_tool(tool.name, arguments=kwargs)
            return FunctionResult(result)

        function = Function(
            name=mcp_tool.name,
            description=mcp_tool.description,
            parameters=mcp_tool.inputSchema.get("properties", {}) if hasattr(mcp_tool, 'inputSchema') else {},
            handler=tool_handler
        )
        agent_functions.append(function)

    # Register MCP tools with the agent
    agent.tools.extend(agent_functions)

    return agent
```

## Success Criteria

- ✅ Ensure that your Python application is running and you are able to debug the application.
- ✅ Ensure that you are able to request the current time and receive an accurate response from the agent.
- ✅ Ensure that you are able to validate policy compliance functionality by ensuring the agent accurately answers travel policy questions.
- ✅ Set a breakpoint in one of the tool handlers and trigger it with a user prompt.
- ✅ Debug and inspect the agent's message history and tool call results.
- ✅ Integrate with the MCP Remote server and receive weather results.
- ✅ Demonstrate that the user can ask questions about weather data through the integrated MCP server.

## Learning Resources
- [Learn Microsoft Agent Framework in 3 minutes!](https://www.youtube.com/watch?v=Q881t44hWng)
- [Introducing Microsoft Agent Framework: The Open-Source Engine for Agentic AI Apps](https://devblogs.microsoft.com/foundry/introducing-microsoft-agent-framework-the-open-source-engine-for-agentic-ai-apps/)
- [Microsoft Agent Framework | MS Learn](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Microsoft Agent Framework | GitHub Repository](https://github.com/microsoft/agent-framework)
- [Microsoft Agent Framework Python Samples | GitHub Repository](https://github.com/microsoft/agent-framework/tree/main/python/samples)
- [Microsoft Agent Framework Python Documentation](https://github.com/microsoft/agent-framework/tree/main/python)
- [Microsoft Agent Framework MCP Integration Guide - Python](https://learn.microsoft.com/en-us/agent-framework/concepts/tools/adding-mcp-tools?pivots=programming-language-python)
