# Challenge 03 Solution - Build Your First MCP Client

This directory contains the complete solution for Challenge 03, which demonstrates how to build a Model Context Protocol (MCP) client in C#.

## What's Included

- **WeatherMcpClient/**: Complete MCP client application that connects to the Weather MCP Server from Challenge 02

## Solution Overview

The MCP client in this solution:

1. **Connects to MCP Servers**: Uses stdio transport to connect to the WeatherMcpServer
2. **Discovers Tools**: Automatically discovers available tools from connected servers
3. **Interactive Interface**: Provides a console-based chat interface for users
4. **Azure OpenAI Integration**: Uses Azure OpenAI for natural language processing
5. **Tool Orchestration**: Automatically calls appropriate tools based on user queries
6. **Natural Language Responses**: Returns user-friendly responses based on tool results

## Key Features Demonstrated

- **MCP Client SDK Usage**: Shows how to use the ModelContextProtocol.Client package
- **Tool Discovery**: Demonstrates listing and converting MCP tools for AI use
- **Azure OpenAI Integration**: Shows integration with Azure OpenAI for tool calling
- **Error Handling**: Implements proper error handling for connection and tool execution
- **Environment Configuration**: Uses environment variables for secure configuration

## Quick Start

1. Navigate to the WeatherMcpClient directory
2. Follow the setup instructions in the README.md
3. Configure your Azure OpenAI credentials
4. Run the application with `dotnet run`

## Architecture

```
Console Interface → Azure OpenAI → Tool Calls → MCP Server → Results → Natural Language Response
```

This solution demonstrates the complete flow of an MCP client application, from user input to natural language output via AI-powered tool orchestration.
