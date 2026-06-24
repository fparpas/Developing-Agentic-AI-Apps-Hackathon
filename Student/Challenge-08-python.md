# Challenge 08 - Python - Optional - Host Your Agent as a Microsoft Foundry Hosted Agent

[< Previous Challenge](./Challenge-07-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-09-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-08-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-08-python.md)

## Introduction

So far you have built Microsoft Agent Framework agents that run as **local console applications**. In this challenge you will take the **weather agent that integrates the remote Weather MCP server** (the one you built in **Challenge 6, Task 2**) and deploy it to the cloud as a **Microsoft Foundry Hosted Agent**.

Hosted agents in Microsoft Foundry Agent Service let you deploy Agent Framework agents as **containerized applications** running on Microsoft-managed infrastructure. Instead of running an agent on your laptop, the platform packages your agent into a container, provisions compute, assigns it a dedicated identity, and exposes it through a managed, OpenAI-compatible endpoint. The platform handles scaling, session state persistence, security, observability, and lifecycle management so you can focus on your agent's logic.

By the end of this challenge you will have taken existing agent code, wrapped it with the Foundry hosting integration, run it locally on `http://localhost:8088`, and deployed it to Foundry Agent Service where it is reachable through its own dedicated endpoint.

## Concepts

### What is a hosted agent?

A **hosted agent** is your own agent code (any framework — Microsoft Agent Framework, Semantic Kernel, LangGraph, or custom code) packaged as a container image and deployed to Foundry Agent Service. This is different from **prompt-based agents**, which are defined entirely through prompts and tool configuration in the Foundry portal.

Choose hosted agents when you want to:

- **Bring your own code** — use any framework rather than prompt-only definitions.
- **Use managed infrastructure** — no need to configure containers, web servers, or scaling rules yourself.
- **Get built-in session management** — the platform persists `$HOME` and uploaded files across turns and idle periods.
- **Get a dedicated agent identity** — every deployed agent receives its own Microsoft Entra identity for secure access to models, tools, and downstream services.
- **Use OpenAI-compatible endpoints** — clients interact with your agent through the Responses protocol using any OpenAI-compatible SDK.

### How it works

You package your agent as a container image and push it to Azure Container Registry. When you deploy, Agent Service pulls the image, provisions compute, assigns a dedicated Microsoft Entra ID (the agent identity), and exposes a dedicated endpoint. At runtime your agent code handles requests and can call Foundry models, tools (including MCP servers), and downstream Azure services using its agent identity. Each session runs in a per-session, VM-isolated sandbox with a persistent filesystem, enabling scale-to-zero with stateful resume.

### Protocols: Responses vs. Invocations

A hosted agent container exposes one or more **protocols**:

- **Responses protocol** *(recommended, used in this challenge)* — exposes an OpenAI-compatible `/responses` endpoint. The platform automatically manages conversation history, streaming events, and session lifecycle. Ideal for conversational chatbots and assistants, which is exactly what our weather agent is.
- **Invocations protocol** — gives you full control over the raw HTTP request and response. Use it for webhooks, non-conversational processing (classification, extraction, batch), or custom streaming protocols that aren't OpenAI-compatible.

### Tooling and runtime environment variables

The **Azure Developer CLI (`azd`)** with the AI agent extension is the easiest way to scaffold, run, and deploy a hosted agent. Alternatively, the **Microsoft Foundry Toolkit for Visual Studio Code** provides the same experience inside VS Code.

When your agent runs inside Foundry, the hosting infrastructure automatically injects these environment variables into your container:

| Variable | Description |
| --- | --- |
| `FOUNDRY_PROJECT_ENDPOINT` | The endpoint URL for the Foundry project. |
| `AZURE_AI_MODEL_DEPLOYMENT_NAME` | The model deployment name (configured during `azd ai agent init`). |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | The Application Insights connection string for telemetry. |

> **Note:** Foundry hosted agents are currently in **preview**. Review the [hosted agents documentation](https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/hosted-agents) for the latest availability, limits, and pricing.

## Description

In this challenge you will create a **new project from scratch** (there is no starter project) that hosts the Challenge 6 weather + remote MCP agent as a Foundry Hosted Agent using the **Responses** protocol.

### Prerequisites

- A [Microsoft Foundry](https://learn.microsoft.com/en-us/azure/foundry/) project with a chat model deployment (for example, `gpt-4o` or `gpt-4.1-mini`).
- The remote Weather MCP server endpoint from **Challenge 4 / Challenge 6** (an HTTP endpoint such as `https://<your-app>.azurecontainerapps.io`).
- [Python 3.10 or later](https://www.python.org/downloads/).
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed and authenticated (`az login`).
- [Azure Developer CLI (`azd`)](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd) with the AI agent extension:
  ```bash
  azd ext install microsoft.foundry
  ```
  *(Alternatively, install the [Microsoft Foundry Toolkit for VS Code](https://aka.ms/foundrytk).)*

### Task 1: Scaffold a hosted agent project

Create an empty folder and initialize a hosted agent project. The fastest way is to start from an Agent Framework sample manifest and select the **Responses** protocol:

```bash
mkdir weather-hosted-agent && cd weather-hosted-agent
azd ai agent init
```

The interactive flow prompts you for an agent name, your Foundry project, subscription, region, and model deployment. When complete, you will have an `azure.yaml` and a starter agent project (typically `main.py` and `requirements.txt`).

> If you prefer VS Code, open the Command Palette and run **Foundry Toolkit: Create new Hosted Agent**, choose **Python**, **Agent Framework**, and the **Responses API** protocol.

Install the hosting package and the Agent Framework into a virtual environment:

```bash
python -m venv .venv
# Windows (PowerShell): .\.venv\Scripts\Activate.ps1
source .venv/bin/activate
pip install agent-framework agent-framework-foundry-hosting azure-identity
```

### Task 2: Host the Challenge 6 weather + MCP agent behind the Responses protocol

Edit `main.py` so that it (1) connects to your **remote Weather MCP server** from Challenge 6, (2) registers the MCP tools on an agent backed by your Foundry model deployment, and (3) exposes that agent through the **Responses** protocol using `ResponsesHostServer`.

```python
import os

from agent_framework import Agent, MCPStreamableHTTPTool
from agent_framework.foundry import FoundryChatClient
from agent_framework_foundry_hosting import ResponsesHostServer
from azure.identity import DefaultAzureCredential

# 1. Create a chat client backed by your Foundry model deployment.
#    FOUNDRY_PROJECT_ENDPOINT and AZURE_AI_MODEL_DEPLOYMENT_NAME are injected
#    automatically when the agent runs in Foundry; set them locally for testing.
client = FoundryChatClient(
    project_endpoint=os.environ["FOUNDRY_PROJECT_ENDPOINT"],
    model=os.environ["AZURE_AI_MODEL_DEPLOYMENT_NAME"],
    credential=DefaultAzureCredential(),
)

# 2. Connect to the remote Weather MCP server from Challenge 6.
weather_mcp = MCPStreamableHTTPTool(
    name="WeatherMCP",
    url=os.environ["WEATHER_MCP_ENDPOINT"],
)

# 3. Build the weather agent and register the MCP tools.
agent = Agent(
    client=client,
    name="weather-hosted-agent",
    instructions="You are a helpful assistant that answers weather questions using the available MCP tools.",
    tools=[weather_mcp],
    # The hosting platform manages conversation history, so don't duplicate it.
    default_options={"store": False},
)

# 4. Host the agent behind the Foundry Responses protocol.
server = ResponsesHostServer(agent)
server.run()
```

`ResponsesHostServer` wraps your agent and exposes it through the Foundry Responses protocol. Setting `store` to `False` avoids duplicating conversation history, since the hosting infrastructure manages history automatically.

### Task 3: Run and test the agent locally

Set the environment variables for local testing and start the agent host:

```bash
export FOUNDRY_PROJECT_ENDPOINT="https://<account>.services.ai.azure.com/api/projects/<project>"
export AZURE_AI_MODEL_DEPLOYMENT_NAME="<your-model-deployment>"
export WEATHER_MCP_ENDPOINT="https://<your-weather-mcp>.azurecontainerapps.io"

azd ai agent run
```

The agent host starts on `http://localhost:8088`. (You can also run `python main.py` directly.) Send it a weather question:

```bash
azd ai agent invoke --local "What is the weather in New York?"
```

Or call the OpenAI-compatible Responses endpoint directly:

```bash
curl -sS -X POST http://localhost:8088/responses \
  -H "Content-Type: application/json" \
  -d '{"input": "What is the weather in New York?", "stream": false}'
```

Confirm that the agent calls the remote MCP weather tool and returns the weather for the requested city.

### Task 4: Deploy to Microsoft Foundry Agent Service

Provision resources (only needed if you don't already have a Foundry project) and deploy:

```bash
azd provision   # creates a Foundry project, model deployment, App Insights, and a container registry
azd deploy       # packages the agent as a container, pushes it to ACR, and deploys it to Foundry
```

When deployment finishes, `azd` prints links to the **agent playground** and the **agent endpoint**. Invoke the deployed agent:

```bash
azd ai agent invoke "What is the weather in Seattle?"
```

Open the **agent playground** in the Foundry portal, ask another weather question, and confirm the hosted agent responds using the remote MCP tools.

> **Clean up:** when you are finished, run `azd down` to delete the resource group and all resources you provisioned so you stop incurring charges.

## Success Criteria

- ✅ Scaffold a hosted agent project using `azd ai agent init` (or the Foundry Toolkit for VS Code) with the **Responses** protocol.
- ✅ Install `agent-framework-foundry-hosting` and wrap an Agent Framework agent with `ResponsesHostServer`.
- ✅ Reuse the Challenge 6 weather agent: connect to the remote Weather MCP server and register its tools on the agent.
- ✅ Run the agent locally on `http://localhost:8088` and get a correct weather answer through `/responses` (via `azd ai agent invoke --local` or `curl`).
- ✅ Deploy the agent to Foundry Agent Service with `azd deploy` and confirm it appears with a dedicated endpoint.
- ✅ Invoke the **deployed** hosted agent and demonstrate it answers weather questions using the MCP tools (via `azd ai agent invoke` or the agent playground).

## Learning Resources

- [Foundry Hosted Agents (Agent Framework) | MS Learn](https://learn.microsoft.com/en-us/agent-framework/hosting/foundry-hosted-agent?pivots=programming-language-python)
- [Hosted agents in Foundry Agent Service – concepts | MS Learn](https://learn.microsoft.com/en-us/azure/foundry/agents/concepts/hosted-agents)
- [Quickstart: Deploy your first hosted agent | MS Learn](https://learn.microsoft.com/en-us/azure/foundry/agents/quickstarts/quickstart-hosted-agent?pivots=vscode)
- [Microsoft Agent Framework Overview | MS Learn](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Foundry hosted agents – Python samples | GitHub](https://github.com/microsoft-foundry/foundry-samples/tree/main/samples/python/hosted-agents/agent-framework)
