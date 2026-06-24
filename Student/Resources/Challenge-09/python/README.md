# Challenge 09 (Python) – Travel Multi-Agent Client

This folder provides a starter project for implementing multi-agent orchestration patterns using the Microsoft Agent Framework. You will complete a travel planning assistant that coordinates multiple specialist agents (coordinator, flight, hotel, activity, transfer, reference, and an optional travel-policy agent) using a handoff workflow pattern. Flight and hotel data is provided through the Travel MCP server that wraps the Amadeus SDK.

## Learning Objectives

- Understand different agent orchestration patterns (Sequential, Concurrent, Handoff, Agents-as-Tools)
- Implement dynamic agent handoff based on conversation context
- Manage shared conversation state across multiple agents
- Integrate persistent agents (Azure AI Foundry) with ephemeral chat agents
- Build context-aware prompts that maintain conversation continuity

## Folder structure

```
python/
├── .env.example                # Copy to .env and fill in credentials
├── requirements.txt            # Both the orchestrator and MCP server dependencies
├── travel_multi_agent_client.py
└── travel_mcp_server/          # Minimal MCP server that proxies Amadeus APIs
    ├── README.md
    ├── requirements.txt        # Same as parent, kept for clarity
    └── server.py
```

## Prerequisites

1. Python 3.11+
2. Azure OpenAI resource with a deployed chat model (e.g. `gpt-4o`)
3. Amadeus for Developers credentials (used by the MCP server)
4. (Optional) Azure AI Foundry project + agent ID if you want the travel policy checker

## Setup

```bash
cd Student/Resources/Challenge-09/python
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\activate
pip install -r requirements.txt
cp .env.example .env       # update with Azure + Amadeus values
```

The travel MCP server runs inside the same process through stdio, so no extra ports are required.  Make sure `AMADEUS_CLIENT_ID/SECRET` are populated in `.env` before launching the client.

## Your Task

Complete the implementation in `travel_multi_agent_client.py` by filling in the TODO sections:

### 1. Agent Initialization (`initialize()` method)
- Load Azure OpenAI settings from environment variables
- Create the `AzureOpenAIResponsesClient`
- Set up the `MCPStdioTool` pointing to the Travel MCP server
- Create agents using `chat_client.as_agent()` with instructions, descriptions, and tools
- Build the handoff workflow with `HandoffBuilder`

### 2. Event Processing (`process_events()` method)
- Consume async workflow events from `run_stream()` or `run()`
- Identify `request_info` events containing `HandoffAgentUserRequest` payloads
- Display agent messages from `event.data.agent_response.messages`
- Collect pending requests for subsequent user responses

### 3. Interaction Loop (`run()` method)
- Implement the main chat loop that handles user input
- Support special commands: `summary`, `policy`, `exit`
- On the first turn: `await self.process_events(self.workflow.run(user_input, stream=True))`
- On subsequent turns: build `responses` dict from `pending_requests` and call `workflow.run(responses=responses, stream=True)`
- The HandoffBuilder manages all agent routing, context broadcast, and handoffs automatically

## Run the orchestrator

Once you've completed the TODO sections, test your implementation:

```bash
python travel_multi_agent_client.py
```

You will be greeted with a console prompt. Type natural language prompts ("Find me a flight from SEA to LHR next June") and the `HandoffBuilder` workflow will automatically route to the appropriate specialist agent. The framework injects handoff tools into each agent, so no manual `HANDOFF:` marker parsing is needed. Type `summary` to see collected trip snippets or `exit` to leave the session.

If `AZURE_AI_FOUNDRY_PROJECT_ENDPOINT` and `AZURE_AI_AGENT_ID` are set in your `.env`, the `policy` command sends the aggregated trip notes to your persistent agent for validation using `DefaultAzureCredential` (so `az login`, managed identity, VS Code identity, etc. are automatically honored).

## Travel MCP server

The embedded server under `travel_mcp_server/` is a trimmed copy of the coach solution.  It exposes two tools:

- `[FLIGHT] search_flight_offers`
- `[HOTEL] search_hotel_offers`

Both rely on the Amadeus SDK, so valid credentials are required.  The server can also be run manually for debugging:

```bash
cd travel_mcp_server
python server.py
```

## Architecture Notes

### Orchestration Patterns

The `agent-framework-orchestrations` package provides high-level builders for common orchestration patterns in both C# and Python:

```python
from agent_framework.orchestrations import (
    SequentialBuilder,   # Agents execute in a fixed order
    ConcurrentBuilder,   # All agents run in parallel
    HandoffBuilder,      # Coordinator dynamically delegates to specialists
    GroupChatBuilder,    # Manager-directed multi-agent conversations
    MagenticBuilder,     # Magentic-One multi-agent orchestration
)
```

This challenge uses **HandoffBuilder** to demonstrate decentralised agent routing.

### HandoffBuilder Details

The `HandoffBuilder` creates a mesh workflow where agents can transfer control to one another:
- Agents are connected automatically - the builder injects handoff tools
- No `HANDOFF:` markers or string parsing needed
- Context is broadcast to all participants after every turn
- The `run(stream=True)` / `run(responses=..., stream=True)` event loop drives the interaction
- Agents decide when to hand off by calling the injected handoff tool

This pattern enables dynamic, context-driven workflows where agents collaborate naturally.

## Next Steps

After completing the core implementation:
- Experiment with the policy agent integration by setting up Azure AI Foundry credentials
- Try building the other orchestration patterns (`SequentialBuilder`, `ConcurrentBuilder`, `GroupChatBuilder`)
- Use `.with_autonomous_mode()` on the `HandoffBuilder` to let agents run without waiting for user input
- Extend the workflow by adding new specialist agents to the `participants` list
- Compare your implementation with the C# version to understand the architectural similarities
