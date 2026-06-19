# Challenge 08 Solution - Host Your Agent as a Microsoft Foundry Hosted Agent (Python)

This directory contains the complete Python coach solution for **Challenge 08**. It takes the weather
agent that integrates the **remote Weather MCP server** (built in Challenge 6, Task 2) and hosts it
as a **Microsoft Foundry Hosted Agent** using the **Responses** protocol.

> Foundry hosted agents are currently in **preview**.

## Project structure

```
python/
├── main.py            # builds the weather+MCP agent and hosts it via the Responses protocol
├── requirements.txt   # agent-framework + agent-framework-foundry-hosting + azure-identity
└── .env.sample        # sample environment variables for local testing
```

## How it works

`main.py` performs four steps:

1. **Creates a chat client** with `FoundryChatClient`, backed by the Foundry model deployment.
   `FOUNDRY_PROJECT_ENDPOINT` and `AZURE_AI_MODEL_DEPLOYMENT_NAME` are injected automatically by the
   hosting platform at runtime; `WEATHER_MCP_ENDPOINT` points at the remote Weather MCP server from
   Challenge 6.
2. **Connects to the remote Weather MCP server** with `MCPStreamableHTTPTool`.
3. **Builds the agent** with the MCP tool registered. `default_options={"store": False}` avoids
   duplicating conversation history, since the hosting platform manages it.
4. **Hosts the agent** with `ResponsesHostServer(agent).run()`, exposing an OpenAI-compatible
   `/responses` endpoint.

## Prerequisites

- [Python 3.10 or later](https://www.python.org/downloads/)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) (`az login`)
- [Azure Developer CLI (`azd`)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd) with the AI agent extension: `azd ext install microsoft.foundry`
- A Microsoft Foundry project with a chat model deployment (for example, `gpt-4o`)
- The remote Weather MCP server endpoint from Challenge 4 / Challenge 6

## Run locally

```bash
cd Coach/Solutions/Challenge-08/python

python -m venv .venv
# Windows (PowerShell): .\.venv\Scripts\Activate.ps1
source .venv/bin/activate
pip install -r requirements.txt

cp .env.sample .env    # then edit .env with your values
azd ai agent run       # or: python main.py
```

The agent host listens on `http://localhost:8088`. Test it:

```bash
azd ai agent invoke --local "What is the weather in New York?"
# or
curl -sS -X POST http://localhost:8088/responses \
  -H "Content-Type: application/json" \
  -d '{"input": "What is the weather in New York?", "stream": false}'
```

## Deploy to Foundry Agent Service

```bash
azd provision   # only if you don't already have a Foundry project
azd deploy
azd ai agent invoke "What is the weather in Seattle?"
```

When you are done, run `azd down` to remove all provisioned resources.

## Learning resources

- [Foundry Hosted Agents (Agent Framework) – Python](https://learn.microsoft.com/en-us/agent-framework/hosting/foundry-hosted-agent?pivots=programming-language-python)
- [Hosted agents in Foundry Agent Service – concepts](https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/hosted-agents)
- [Quickstart: Deploy your first hosted agent](https://learn.microsoft.com/en-us/azure/foundry/agents/quickstarts/quickstart-hosted-agent?pivots=vscode)
