# Secure Weather MCP Client

A client application that demonstrates how to connect to the secure weather MCP server using API key authentication.

## Features

- **Azure OpenAI Integration**: Uses Azure OpenAI for natural language processing
- **Secure MCP Communication**: Connects to MCP servers with API key authentication
- **Interactive Interface**: Command-line interface for weather queries
- **Function Calling**: Automatic tool execution based on user queries
- **Configuration Management**: Secure configuration with user secrets support

## Setup

### Prerequisites

- .NET 9.0 SDK
- Azure OpenAI service endpoint and API key
- Running Secure Weather MCP Server

### Configuration

1. Update `appsettings.Development.json` with your Azure OpenAI credentials:
   ```json
   {
     "AzureOpenAI": {
       "Endpoint": "https://your-openai.openai.azure.com/",
       "ApiKey": "your-api-key",
       "DeploymentName": "gpt-4o"
     }
   }
   ```

2. Ensure the MCP server is running on `http://localhost:5000`

3. The client uses the default API key: `sk-demo-weather-api-key-12345`

### Running the Client

```bash
cd SecureWeatherMcpClient
dotnet run
```

## Usage

The client provides an interactive interface where you can ask weather-related questions:

- "What's the weather in London?"
- "Give me a 5-day forecast for New York"
- "How's the weather in Tokyo in Fahrenheit?"

The client will automatically:
1. Connect to the secure MCP server using API key authentication
2. Get available weather tools
3. Use Azure OpenAI to understand your query
4. Execute the appropriate weather tools
5. Present the results in natural language

## Architecture

The client demonstrates:
- **Secure HTTP Transport**: API key authentication with the MCP server
- **Function Calling**: Integration between Azure OpenAI and MCP tools
- **Error Handling**: Proper error handling for authentication and network issues
- **Configuration Security**: Separation of development and production configurations

## Security Features

- API key authentication for MCP server access
- Secure configuration management
- User secrets support for development
- No hardcoded credentials in source code
