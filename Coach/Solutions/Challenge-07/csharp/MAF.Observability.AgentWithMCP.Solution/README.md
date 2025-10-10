# Semantic Kernel with MCP Integration

This project demonstrates how to integrate Microsoft Semantic Kernel with Model Context Protocol (MCP) tools, creating a powerful AI agent that can leverage external tools and services.

## Overview

This console application combines the power of:
- **Microsoft Semantic Kernel**: A lightweight SDK for AI orchestration with .NET
- **Model Context Protocol (MCP)**: A standardized protocol for connecting AI assistants to external tools and data sources
- **Azure OpenAI**: For natural language processing and AI capabilities

## Features

- ✅ Semantic Kernel integration with Azure OpenAI
- ✅ Dynamic MCP tool discovery and registration
- ✅ Automatic tool invocation through Semantic Kernel
- ✅ Interactive chat interface
- ✅ Support for both local and remote MCP servers
- ✅ Comprehensive logging and error handling

## Prerequisites

- .NET 8.0 SDK
- Azure OpenAI service with a deployed model (e.g., GPT-4)
- Access to MCP servers (local or remote)

## Configuration

Update the `appsettings.json` file with your Azure OpenAI credentials and MCP server settings:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-endpoint.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4o"
  },
  "MCPServer": {
    "UseLocalMCP": true,
    "LocalMCP": {
      "Name": "WeatherMcpServer",
      "Command": "dotnet",
      "Arguments": [
        "run",
        "--project",
        "..\\..\\Challenge-02\\WeatherMcpServer\\WeatherMcpServer.csproj"
      ]
    },
    "RemoteMCP": {
      "Endpoint": "http://localhost:5000/sse"
    }
  }
}
```

## Usage

### Running the Application

1. **Clone and navigate to the project directory**:
   ```bash
   cd Coach/Solutions/Challenge-06/SemanticKernelWithMCP
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Update configuration** with your Azure OpenAI credentials in `appsettings.json`

4. **Run the application**:
   ```bash
   dotnet run
   ```

### Example Interactions

Once the application starts, you can interact with it using natural language:

```
You: What's the current weather in London?
Assistant: [Uses MCP weather tool to get real-time data]

You: Can you get weather for multiple cities?
Assistant: [Automatically invokes appropriate MCP tools]

You: exit
```

## Architecture

### Key Components

1. **Program.cs**: Main application entry point and orchestration
2. **Kernel Configuration**: Sets up Semantic Kernel with Azure OpenAI
3. **MCP Client Integration**: Connects to MCP servers and discovers tools
4. **Function Registration**: Converts MCP tools to Semantic Kernel functions
5. **Interactive Chat Loop**: Handles user interactions and tool invocations

### MCP Integration Flow

1. **Initialize MCP Client**: Connect to local or remote MCP server
2. **Discover Tools**: Query available tools from MCP server
3. **Register Functions**: Convert MCP tools to Semantic Kernel functions
4. **Auto-Invoke**: Let Semantic Kernel automatically call tools when needed
5. **Return Results**: Process and display results to the user

## Project Structure

```
SemanticKernelWithMCP/
├── Program.cs                    # Main application logic
├── SemanticKernelWithMCP.csproj  # Project configuration and dependencies
├── appsettings.json              # Configuration (production)
├── appsettings.Development.json  # Configuration (development)
└── README.md                     # This documentation
```

## Key Dependencies

- **Microsoft.SemanticKernel**: Core SK functionality
- **Microsoft.SemanticKernel.Connectors.AzureOpenAI**: Azure OpenAI integration
- **ModelContextProtocol.Client**: MCP client library
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.Logging**: Logging infrastructure

## MCP Tool Integration

The application automatically:

1. **Discovers** all available tools from the connected MCP server
2. **Registers** each MCP tool as a Semantic Kernel function
3. **Enables** automatic tool invocation based on user queries
4. **Handles** tool execution and result processing

### Supported MCP Transports

- **StdioClientTransport**: For local MCP servers running as processes
- **SseClientTransport**: For remote MCP servers using Server-Sent Events

## Error Handling

The application includes comprehensive error handling for:
- Configuration validation
- MCP server connection issues
- Azure OpenAI API errors
- Tool execution failures
- JSON serialization/deserialization errors

## Logging

Configured with structured logging to help with debugging and monitoring:
- Application lifecycle events
- MCP tool discovery and execution
- Error conditions and exceptions
- User interactions and responses

## Extending the Application

To add more functionality:

1. **Add new MCP servers**: Update configuration to connect to additional MCP servers
2. **Custom functions**: Register additional Semantic Kernel functions alongside MCP tools
3. **Advanced prompts**: Implement more sophisticated prompt engineering
4. **Memory integration**: Add conversation memory and context management

## Troubleshooting

### Common Issues

1. **Configuration errors**: Verify Azure OpenAI credentials and MCP server settings
2. **MCP connection failures**: Ensure MCP servers are running and accessible
3. **Tool execution errors**: Check MCP server logs for detailed error information

### Debug Mode

Set `"LogLevel": { "Default": "Debug" }` in configuration for verbose logging.

## Learning Resources

- [Microsoft Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/docs)
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

---

This application demonstrates the powerful combination of Semantic Kernel's AI orchestration capabilities with the extensibility of Model Context Protocol tools, creating a flexible and scalable AI agent architecture.