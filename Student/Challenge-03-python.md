# Challenge 03 - Python - Build your first MCP client

 [< Previous Challenge](./Challenge-02-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-04-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-03-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-03-python.md)

## Introduction

In this challenge, you will build your first Model Context Protocol (MCP) client using Python and the Microsoft Agent Framework. While in the previous challenge you created an MCP server that provides tools, now you'll create an agent client that can connect to MCP servers, discover their capabilities, and interact with their tools through an AI assistant.

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

In this challenge, you will build a Python agent application using the Microsoft Agent Framework that can connect to your Weather MCP Server from the previous challenge (or any other MCP server) and provide an interactive interface where users can ask questions that leverage the server's tools.

### Task 1: Set up your environment

**For faster dependency management, consider using `uv`:** [`uv` is an extremely fast Python package installer and resolver](https://docs.astral.sh/uv/). It's significantly faster than `pip` (10-100x in many cases) and handles dependency resolution more efficiently. You can install it from https://docs.astral.sh/uv/getting-started/installation/.

Create a new directory for the project:
```bash
mkdir mcp_weather_client
cd mcp_weather_client
```

Create a `requirements.txt` file in your project:
```plaintxt
# MCP Weather Client Agent Dependencies
# Python 3.10+ required

# Microsoft Agent Framework (includes pre-release Azure AI Agents)
agent-framework>=0.1.0
azure-ai-agents>=1.2.0b5

# Model Context Protocol SDK
mcp[cli]>=1.2.0

# Environment variable management
python-dotenv>=1.0.0
```

Create a new Python project for the MCP client agent:

**Using `uv` (recommended for performance):**
```bash
# Create a virtual environment
uv venv .venv

# Activate the virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install the Agent Framework and dependencies
uv pip install -r requirements.txt
```

**Or using standard `pip`:**
```bash
# Create a virtual environment
python -m venv .venv

# Activate the virtual environment
source .venv/bin/activate  # On Windows: .venv\Scripts\activate

# Install the Agent Framework and dependencies
pip install -r requirements.txt
```

### Task 2: Configure Azure OpenAI and environment variables

**Prerequisites:**
1. Have an Azure OpenAI resource deployed with a chat model (e.g., gpt-4o or gpt-4o-mini)
2. Have the following information:
   - Endpoint URL
   - API Key
   - Deployment name
   - API version

Create a `.env` file in your project:

```env
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your_azure_openai_api_key_here
AZURE_OPENAI_DEPLOYMENT_NAME=your_deployment_name
AZURE_OPENAI_API_VERSION=latest
```

You can start from the provided `.env.sample` file.

> ⚠️ **Never commit your .env file to version control!** Add it to `.gitignore`.

### Task 3: Create an MCP client agent

The Microsoft Agent Framework provides `MCPStdioTool` to simplify connecting to MCP servers. This automatically handles tool discovery and execution. Create a file named `mcp_weather_client.py`:

```python
"""
MCP Weather Client Agent - Challenge 03

This module demonstrates integrating MCP tools with the Agent Framework
using MCPStdioTool for easy tool discovery and execution from MCP servers.
"""

import asyncio
import os
import sys
from agent_framework import ChatAgent
from agent_framework.azure import AzureOpenAIResponsesClient
from agent_framework import MCPStdioTool
from dotenv import load_dotenv

# Load environment variables
load_dotenv()


async def main():
    """Main entry point for the MCP weather client agent."""
    if len(sys.argv) < 2:
        print("Usage: python mcp_weather_client.py <path_to_weather_server.py>")
        print("\nExample:")
        print("  python mcp_weather_client.py ../../Challenge-02/python/weather.py")
        sys.exit(1)

    server_path = sys.argv[1]

    # Verify the server file exists
    if not os.path.exists(server_path):
        print(f"Error: Server file not found: {server_path}")
        sys.exit(1)

    # Get Azure OpenAI configuration
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    api_key = os.getenv("AZURE_OPENAI_API_KEY")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    api_version = os.getenv("AZURE_OPENAI_API_VERSION")

    # Validate configuration
    if not all([endpoint, api_key, deployment_name, api_version]):
        missing = [
            var for var, val in [
                ("AZURE_OPENAI_ENDPOINT", endpoint),
                ("AZURE_OPENAI_API_KEY", api_key),
                ("AZURE_OPENAI_DEPLOYMENT_NAME", deployment_name),
                ("AZURE_OPENAI_API_VERSION", api_version),
            ] if not val
        ]
        print("Error: Missing required environment variables:")
        for var in missing:
            print(f"  - {var}")
        sys.exit(1)

    # Create Azure OpenAI chat client
    chat_client = AzureOpenAIResponsesClient(
        endpoint=endpoint,
        api_key=api_key,
        deployment_name=deployment_name,
        api_version=api_version
    )

    # Create MCP tool for the weather server
    mcp_tool = MCPStdioTool(
        name="WeatherMCP",
        command="python",
        args=[server_path]
    )

    # Create agent with MCP tools
    async with ChatAgent(
        chat_client=chat_client,
        name="WeatherAgent",
        instructions="You are a helpful weather assistant. Use the available MCP tools to answer weather questions accurately.",
        tools=[mcp_tool]
    ) as agent:
        print("\n" + "=" * 60)
        print("Weather Assistant")
        print("=" * 60)
        print("Ask me about the weather. Type 'exit' to quit.\n")

        # Interactive chat loop
        while True:
            try:
                user_input = input("You: ").strip()

                if user_input.lower() in ("exit", "quit", "bye"):
                    print("Goodbye!")
                    break

                if not user_input:
                    continue

                print("Agent: ", end="", flush=True)
                response = await agent.run(user_input)
                print(response)
                print()

            except KeyboardInterrupt:
                print("\n\nSession ended.")
                break
            except Exception as e:
                print(f"Error: {str(e)}\n")


if __name__ == "__main__":
    asyncio.run(main())
```

## Resulting Project Structure
```
mcp_weather_client/
├── mcp_weather_client.py     # Main agent client implementation
├── requirements.txt          # Python dependencies
├── .env                      # Environment variables
└── .venv/                    # Python Virtual Environment (created during setup)
```

## Success Criteria

- ✅ A Python agent application using the Microsoft Agent Framework that can connect to MCP servers via stdio
- ✅ Agent can connect to and discover tools from connected MCP servers
- ✅ Interactive interface for user queries
- ✅ Integration with Azure OpenAI chat client for natural language processing
- ✅ Agent can reason about which tools to use based on user queries
- ✅ Agent can execute tool calls on MCP servers and return results
- ✅ User can ask weather-related questions and get natural language responses
- ✅ Agent works with the Weather MCP Server from Challenge 02

## Running Your Agent

In a terminal window, run your agent client. The first and only argument
that you need to pass is the path to the weather MCP server from Challenge-02.
The client will start the MCP server automatically.

```bash
python mcp_weather_client.py weather.py
```

Expected output:
```
[10/24/25 16:34:00] INFO     Processing request of type ListToolsRequest              server.py:674
                    INFO     Processing request of type ListPromptsRequest            server.py:674

============================================================
Weather Assistant
============================================================
Ask me about the weather. Type 'exit' to quit.

You: Is it going to rain tomorrow in Seattle?
Agent: [10/24/25 15:58:05] INFO     Processing request of type CallToolRequest                                              server.py:674
[10/24/25 15:58:08] INFO     HTTP Request: GET https://api.weather.gov/points/47.6062,-122.3321 "HTTP/1.1 200 OK"           _client.py:1740
[10/24/25 15:58:09] INFO     HTTP Request: GET https://api.weather.gov/gridpoints/SEW/125,68/forecast "HTTP/1.1 200 OK"     _client.py:1740

Yes, it's going to rain in Seattle tomorrow.

- **Friday Forecast**:
  - High Temperature: 61°F
  - Winds: 9 to 13 mph SSW
  - Forecast: Rain expected, with a 100% chance of precipitation. New rainfall amounts between a quarter and half of an inch possible.

```

Try queries like:
- "What's the weather in Sacramento?"
- "Are there any weather alerts for California?"
- "Give me the forecast for Seattle"
- "Tell me about weather alerts in Texas"

## Implementation Architecture

### Client Agent Flow

The agent follows this flow when processing user queries:

1. User submits a natural language query
2. Agent receives the query and analyzes available tools
3. Azure OpenAI LLM decides which tools to use
4. MCPStdioTool executes tool calls on the MCP server
5. Results are processed by the agent
6. Natural language response is generated
7. Response is returned to the user

### Key Components

**MCPStdioTool Integration**
- Creates a tool that wraps the MCP server using stdio transport
- Automatically discovers and manages tools from the MCP server
- Seamlessly integrates MCP tools with the Agent Framework

**ChatAgent Integration**
- Uses Microsoft Agent Framework ChatAgent for agent orchestration
- Integrates with Azure OpenAI for language understanding
- Processes user queries and generates responses
- Automatically manages tool selection and execution

**Error Handling**
- Environment variable validation with clear error messages
- Server file existence checking
- Interactive error recovery with try-except blocks
- Graceful handling of interrupt signals (Ctrl+C)

## Extending the Client

### Using Different LLM Providers

Switch between providers by using different chat clients:

```python
# Azure OpenAI (current implementation)
from agent_framework.azure import AzureOpenAIResponsesClient

# OpenAI
from agent_framework.openai import OpenAIResponsesClient

# Anthropic
from agent_framework.anthropic import AnthropicChatClient
```

### Connecting to Multiple MCP Servers

Create multiple MCPStdioTool instances and pass them to the agent:

```python
weather_tool = MCPStdioTool(
    name="WeatherMCP",
    command="python",
    args=[weather_server_path]
)

news_tool = MCPStdioTool(
    name="NewsMCP",
    command="python",
    args=[news_server_path]
)

async with ChatAgent(
    chat_client=chat_client,
    name="MultiToolAgent",
    instructions="You have access to weather and news tools...",
    tools=[weather_tool, news_tool]
) as agent:
    # Use both tools
```

## Troubleshooting

**Missing Command-Line Arguments**
- Run with: `python mcp_weather_client.py <path_to_weather_server.py>`
- Example: `python mcp_weather_client.py ../../Challenge-02/python/weather.py`

**Server File Not Found**
- Verify the path to the weather server is correct and relative to your current directory
- Use absolute paths if you encounter issues with relative paths

**Missing Environment Variables**
- Verify all required Azure OpenAI settings are in `.env` file:
  - `AZURE_OPENAI_ENDPOINT`
  - `AZURE_OPENAI_API_KEY`
  - `AZURE_OPENAI_DEPLOYMENT_NAME`
  - `AZURE_OPENAI_API_VERSION`
- Check that `.env` file is in the current working directory. For convenience, we provide a `.env.sample` file.

**Connection Issues**
- MCPStdioTool will automatically start the server, so you don't need to run it separately
- Check that Python 3.10+ is being used
- Verify the server script is executable and valid

**Import Errors**
- Reinstall dependencies: `pip install -r requirements.txt`
- Verify virtual environment is activated: `source .venv/bin/activate`
- Check that agent-framework is installed: `pip list | grep agent-framework`

**Agent Not Responding**
- Verify Azure OpenAI API key is valid and has available quota
- Check network connectivity to Azure OpenAI endpoint
- Review error messages in console output

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [MCP Python SDK Documentation](https://modelcontextprotocol.io/docs/sdk/python)
- [MCP Client Quickstart](https://modelcontextprotocol.io/quickstart/client)
- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Agent Framework Python Samples](https://github.com/microsoft/agent-framework/tree/main/python/samples)
- [OpenAI API Documentation](https://platform.openai.com/docs)
- [MCP Architecture](https://modelcontextprotocol.io/legacy/concepts/architecture)
- [Deep Dive: Understanding MCP Client-Server Communication with Agent and LLM](https://medium.com/@jamestang/deep-dive-understanding-mcp-client-server-communication-with-agent-and-llm-aa4782a65991)
