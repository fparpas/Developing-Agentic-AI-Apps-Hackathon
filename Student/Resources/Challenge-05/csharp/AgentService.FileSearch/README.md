# Azure Agent Service File Search

A C# console application that connects to Azure OpenAI Agent Service and performs file search operations using vector stores.

## Features

- 🔍 **Simple Search**: Quick text-based file searches
- 🔧 **Advanced Search**: Search with file type filters and result limits
- 📊 **Rich Results**: Displays file content, metadata, and relevance scores
- ⚡ **Real-time Search**: Interactive console interface
- 🔐 **Secure Configuration**: Uses Azure Key Vault and user secrets

## Prerequisites

- .NET 8.0 SDK
- Azure OpenAI resource with Assistants API enabled
- Azure OpenAI Assistant configured with file search capabilities
- Vector store with uploaded files

## Setup

### 1. Configure Azure OpenAI

You need to set up the following in your Azure OpenAI resource:
- Create an Assistant with file search tools enabled
- Create a Vector Store and upload files to it
- Note down the Assistant ID and Vector Store ID

### 2. Configure Application Settings

#### Option A: User Secrets (Recommended for development)

```bash
cd AgentServiceFileSearch
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:AssistantId" "your-assistant-id"
dotnet user-secrets set "AzureOpenAI:VectorStoreId" "your-vector-store-id"
```

#### Option B: Environment Variables

```bash
set AZUREOPENAI__ENDPOINT=https://your-resource.openai.azure.com/
set AZUREOPENAI__APIKEY=your-api-key
set AZUREOPENAI__ASSISTANTID=your-assistant-id
set AZUREOPENAI__VECTORSTOREID=your-vector-store-id
```

#### Option C: appsettings.json (Not recommended for production)

Update the `appsettings.json` file:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "ModelId": "gpt-4o",
    "AssistantId": "your-assistant-id",
    "VectorStoreId": "your-vector-store-id"
  }
}
```

### 3. Build and Run

```bash
dotnet build
dotnet run
```

## Usage

### Simple Search
Just type your search query:
```
🔍 Search> machine learning algorithms
```

### Advanced Search
Use the advanced search command for more control:
```
🔍 Search> advanced
Search query: neural networks
Max results (default 10): 5
File types (comma-separated, e.g., pdf,txt,docx): pdf,docx
```

### Commands
- `help` - Show available commands
- `advanced` - Advanced search with filters
- `exit` - Quit the application

## Configuration Options

### appsettings.json
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "ModelId": "gpt-4o",
    "AssistantId": "your-assistant-id",
    "VectorStoreId": "your-vector-store-id"
  },
  "FileSearch": {
    "MaxResults": 10,
    "SearchTimeout": 30000
  }
}
```

### Search Configuration
- `MaxResults`: Default maximum number of results to return
- `SearchTimeout`: Timeout in milliseconds for search operations

## Architecture

### Services
- **AzureAgentService**: Handles Azure OpenAI Assistants API communication
- **VectorStoreService**: Manages vector store search operations

### Models
- **FileSearchRequest**: Search request parameters
- **FileSearchResult**: Individual search result
- **FileSearchResponse**: Complete search response with metadata

## Troubleshooting

### Common Issues

1. **Connection Failed**
   - Verify your Azure OpenAI endpoint and API key
   - Ensure the Assistants API is enabled in your region

2. **No Search Results**
   - Verify your Vector Store ID is correct
   - Ensure files are uploaded to the vector store
   - Check that your Assistant has file search tools enabled

3. **Assistant Not Found**
   - Verify your Assistant ID is correct
   - Ensure the assistant exists in your Azure OpenAI resource

### Logging
The application uses structured logging. Set the log level in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AgentServiceFileSearch": "Debug"
    }
  }
}
```

## Development

### Adding New Features
1. Create new models in the `Models` folder
2. Add service methods to `AzureAgentService` or `VectorStoreService`
3. Update the console interface in `Program.cs`

### Testing
- Use the connection test feature to verify Azure OpenAI connectivity
- Test with various file types and search queries
- Monitor logs for performance and error information

## License

This project is part of the Developing Agentic AI Apps Hackathon challenges.