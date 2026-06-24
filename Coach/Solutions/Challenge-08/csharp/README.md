# Challenge 08 Solution - Host Your Agent as a Microsoft Foundry Hosted Agent (C#)

This directory contains the complete C# coach solution for **Challenge 08**. It takes the weather
agent that integrates the **remote Weather MCP server** (built in Challenge 6, Task 2) and hosts it
as a **Microsoft Foundry Hosted Agent** using the **Responses** protocol.

> Foundry hosted agents are currently in **preview**.

## Project structure

```
csharp/
└── MAF.HostedAgent.Solution/
    ├── MAF.HostedAgent.Solution.csproj   # net10.0 + Foundry hosting + MCP packages
    └── Program.cs                        # builds the weather+MCP agent and hosts it via Responses
```

## How it works

`Program.cs` performs four steps:

1. **Reads configuration** from environment variables. `FOUNDRY_PROJECT_ENDPOINT` and
   `AZURE_AI_MODEL_DEPLOYMENT_NAME` are injected automatically by the hosting platform at runtime;
   `WEATHER_MCP_ENDPOINT` points at the remote Weather MCP server from Challenge 6.
2. **Connects to the remote Weather MCP server** with `McpClient` over `HttpClientTransport` and
   lists its tools.
3. **Builds the agent** on top of the Foundry model deployment with
   `new AIProjectClient(...).AsAIAgent(model, instructions, name, tools)`, registering the MCP tools.
4. **Hosts the agent** behind the Foundry Responses protocol with `AgentHost.CreateBuilder`,
   `AddFoundryResponses`, and `MapFoundryResponses`, which exposes an OpenAI-compatible `/responses`
   endpoint.

## Key packages

| Package | Purpose |
| --- | --- |
| `Microsoft.Agents.AI.Foundry.Hosting` | `AgentHost`, `AddFoundryResponses`, `MapFoundryResponses` |
| `Azure.AI.Projects` | `AIProjectClient` and the `AsAIAgent` bridge to an `AIAgent` |
| `Microsoft.Agents.AI` | The `AIAgent` abstraction |
| `ModelContextProtocol` | `McpClient` / `HttpClientTransport` to connect to the remote MCP server |
| `Azure.Identity` | `DefaultAzureCredential` for the agent identity |

> **Note:** Only one of `Azure.AI.Projects` and `Microsoft.Agents.AI.AzureAI` should be referenced —
> both expose an `AsAIAgent(AIProjectClient, ...)` extension and referencing both makes the call
> ambiguous (`CS0121`). This solution uses `Azure.AI.Projects`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (`az login`)
- [Azure Developer CLI (`azd`)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd) with the AI agent extension: `azd ext install microsoft.foundry`
- A Microsoft Foundry project with a chat model deployment (for example, `gpt-4o`)
- The remote Weather MCP server endpoint from Challenge 4 / Challenge 6

## Run locally

```bash
# Windows (PowerShell): use $env:NAME = "value" instead of export
export FOUNDRY_PROJECT_ENDPOINT="https://<account>.services.ai.azure.com/api/projects/<project>"
export AZURE_AI_MODEL_DEPLOYMENT_NAME="gpt-4o"
export WEATHER_MCP_ENDPOINT="https://<your-weather-mcp>.azurecontainerapps.io"

cd MAF.HostedAgent.Solution
azd ai agent run            # or: dotnet run
```

The agent host listens on `http://localhost:8088`. Test it:

```bash
azd ai agent invoke --local "What is the weather in New York?"
# or
curl -X POST http://localhost:8088/responses \
  -H "Content-Type: application/json" \
  -d '{"input": "What is the weather in New York?"}'
```

## Deploy to Foundry Agent Service

```bash
azd provision   # only if you don't already have a Foundry project
azd deploy
azd ai agent invoke "What is the weather in Seattle?"
```

When you are done, run `azd down` to remove all provisioned resources.

## Learning resources

- [Foundry Hosted Agents (Agent Framework) – C#](https://learn.microsoft.com/en-us/agent-framework/hosting/foundry-hosted-agent?pivots=programming-language-csharp)
- [Hosted agents in Foundry Agent Service – concepts](https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/hosted-agents)
- [Quickstart: Deploy your first hosted agent](https://learn.microsoft.com/en-us/azure/foundry/agents/quickstarts/quickstart-hosted-agent?pivots=vscode)
