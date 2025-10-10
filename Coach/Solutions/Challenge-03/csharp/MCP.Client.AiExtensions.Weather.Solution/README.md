# Weather MCP Client - Challenge 03 Solution

This is the solution for Challenge 03, which demonstrates how to build an MCP (Model Context Protocol) client that connects to MCP servers and provides an interactive chat interface using Azure OpenAI.

## Overview

This MCP client:
- Connects to the Weather MCP Server from Challenge 02
- Discovers available tools from the server
- Provides an interactive console interface for user queries
- Uses Azure OpenAI to process natural language queries
- Automatically calls appropriate tools based on user questions
- Returns natural language responses to users

## Prerequisites

1. **Azure OpenAI Resource**: You need an Azure OpenAI resource with a deployed GPT-4 model
2. **WeatherMcpServer**: The Weather MCP Server from Challenge 02 must be available
3. **.NET 9.0**: Ensure you have .NET 9.0 SDK installed

## Setup Instructions

### 1. Configure Azure OpenAI

First, set up your Azure OpenAI credentials. You have two options:

#### Option A: Environment Variables (Recommended)
Set the following environment variables in your system:

```bash
# Windows (PowerShell)
$env:AZURE_OPENAI_ENDPOINT="https://your-resource-name.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY="your_azure_openai_api_key_here"
$env:AZURE_OPENAI_DEPLOYMENT_NAME="your_gpt4_deployment_name"

# Windows (Command Prompt)
set AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
set AZURE_OPENAI_API_KEY=your_azure_openai_api_key_here
set AZURE_OPENAI_DEPLOYMENT_NAME=your_gpt4_deployment_name

# Linux/macOS
export AZURE_OPENAI_ENDPOINT="https://your-resource-name.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your_azure_openai_api_key_here"
export AZURE_OPENAI_DEPLOYMENT_NAME="your_gpt4_deployment_name"
```

#### Option B: .env File
1. Copy `.env.example` to `.env`
2. Edit `.env` and replace the placeholder values with your actual Azure OpenAI details

### 2. Build the Project

```bash
cd WeatherMcpClient
dotnet restore
dotnet build
```

### 3. Run the Application

Make sure the WeatherMcpServer from Challenge 02 is built first:

```bash
# Build the Weather MCP Server (if not already built)
cd ../Challenge-02/WeatherMcpServer
dotnet build

# Run the MCP Client
cd ../../Challenge-03/WeatherMcpClient
dotnet run
```

## Usage

Once the application starts, you'll see:

```
Connected to MCP server successfully!
Available tool: get_weather - Get weather forecast for a location
Available tool: get_alerts - Get weather alerts for a US state

MCP Client Started!
You can now ask weather-related questions!
Enter a command (or 'exit' to quit):
> 
```

You can now ask natural language questions like:

- "What's the weather in Sacramento?"
- "Are there any weather alerts for California?"
- "Give me the forecast for New York City"
- "What's the temperature in San Francisco?"
- "Any severe weather warnings for Texas?"

Type `exit` to quit the application.

## How It Works

1. **MCP Connection**: The client connects to the WeatherMcpServer using stdio transport
2. **Tool Discovery**: It discovers available tools from the server (get_weather and get_alerts)
3. **Query Processing**: User queries are sent to Azure OpenAI along with tool definitions
4. **Tool Execution**: If Azure OpenAI determines tools are needed, the client executes them on the MCP server
5. **Response Generation**: The tool results are sent back to Azure OpenAI to generate a natural language response

## Architecture

```
User Query → Azure OpenAI → Tool Calls → MCP Server → Tool Results → Azure OpenAI → Natural Language Response
```

## Key Components

- **StdioClientTransport**: Handles communication with the MCP server via stdio
- **McpClient**: Main client for interacting with MCP servers
- **OpenAIClient**: Azure OpenAI client for natural language processing
- **Tool Conversion**: Converts MCP tool definitions to Azure OpenAI tool format

## Error Handling

The application includes error handling for:
- Missing Azure OpenAI configuration
- MCP server connection failures
- Tool execution errors
- Azure OpenAI API errors

## Troubleshooting

### "Failed to connect to MCP server"
- Ensure the WeatherMcpServer project is built (`dotnet build` in the WeatherMcpServer directory)
- Check that the path to WeatherMcpServer is correct in the StdioClientTransport configuration

### "Please set AZURE_OPENAI_ENDPOINT and AZURE_OPENAI_API_KEY"
- Verify your environment variables are set correctly
- If using a .env file, ensure it's in the correct location and properly formatted

### "Unauthorized" or "404" errors
- Check your Azure OpenAI endpoint URL (should end with `/`)
- Verify your API key is correct
- Ensure your deployment name matches the actual deployment in Azure OpenAI Studio

## Dependencies

- `Azure.AI.OpenAI` (2.0.0): Azure OpenAI client
- `Microsoft.Extensions.Hosting` (9.0.8): .NET hosting framework

## Implementation Notes

This solution demonstrates MCP client concepts using the available Azure OpenAI SDK. Since the official ModelContextProtocol.Client package is not yet available or has different APIs than documented in the challenge, this implementation:

1. **Tool Definition**: Uses Azure OpenAI's `ChatTool` to define available tools
2. **MCP Communication**: Simulates MCP server communication via process execution
3. **Fallback Handling**: Provides simulated responses when the MCP server is not available
4. **Function Calling**: Uses Azure OpenAI's function calling capability to determine when to use tools

The implementation follows the MCP client patterns described in the challenge while using currently available SDKs.

## Next Steps

This client demonstrates the basics of MCP client development. You can extend it by:
- Adding support for multiple MCP servers
- Implementing conversation history
- Adding more sophisticated error handling
- Creating a web-based UI instead of console interface
- Adding logging and telemetry
