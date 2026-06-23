# Azure Agent Service File Search

A C# console application that connects to Azure OpenAI Agent Service and performs file search operations using vector stores.

## Features

- 🔍 **Simple Search**: Quick text-based file searches
- 🔧 **Advanced Search**: Search with file type filters and result limits
- 📊 **Rich Results**: Displays file content, metadata, and relevance scores
- ⚡ **Real-time Search**: Interactive console interface
- 🔐 **Secure Configuration**: Uses Azure Key Vault and user secrets

## Prerequisites

- .NET 9.0 SDK
- Azure AI Foundry project with an Agent configured with file search capabilities
- Vector store with uploaded files

## Setup

### 1. Configure Azure AI Foundry

You need to set up the following in your Azure AI Foundry project:
- Create an Agent with file search tools enabled
- Create a Vector Store and upload files to it
- Note down the Agent Name and project endpoint

### 2. Configure Application Settings

#### Option A: User Secrets (Recommended for development)

```bash
cd AgentService.NewFoundry.FileSearch.Solution
dotnet user-secrets set "AgentService:Endpoint" "https://your-resource.services.ai.azure.com/api/projects/your-project"
dotnet user-secrets set "AgentService:AgentName" "your-agent-name"
```

#### Option B: appsettings.json

Update the `appsettings.json` file:

```json
{
  "AgentService": {
    "Endpoint": "https://your-resource.services.ai.azure.com/api/projects/your-project",
    "AgentName": "your-agent-name"
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

## Key Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Azure.AI.Projects` | 2.0.1 | Azure AI Foundry project client |
| `Azure.AI.Extensions.OpenAI` | 2.0.0 | Responses API (ProjectResponsesClient, conversations) |
| `Azure.AI.Projects.Agents` | 2.0.0 | Agent administration (ProjectsAgentRecord) |
| `Azure.Identity` | 1.21.0 | DefaultAzureCredential authentication |

## Architecture

The application uses the Azure AI Foundry v2 SDK:
- **AIProjectClient** → connects to the Foundry project
- **AgentAdministrationClient** → retrieves the agent by name
- **ProjectOpenAIClient** → creates conversations and response clients
- **ProjectResponsesClient** → sends user messages and receives agent responses

## Troubleshooting

### Common Issues

1. **Connection Failed**
   - Verify your Azure AI Foundry project endpoint
   - Ensure you are logged in with `az login` (DefaultAzureCredential is used)

2. **No Search Results**
   - Ensure files are uploaded to the vector store linked to your agent
   - Check that your Agent has file search tools enabled

3. **Agent Not Found**
   - Verify your Agent name is correct in `appsettings.json`
   - Ensure the agent exists in your Azure AI Foundry project

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