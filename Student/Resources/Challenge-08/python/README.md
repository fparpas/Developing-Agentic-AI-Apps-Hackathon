# Challenge 08 (Python) – Travel Multi-Agent Client

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
cd Student/Resources/Challenge-08/python
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
- Create `ChatAgent` instances for each agent specification
- Initialize threads for each agent
- Set the coordinator as the starting agent

### 2. Handoff Workflow (`run()` method)
- Implement the main chat loop that handles user input
- Support special commands: `summary`, `policy`, `exit`
- Manage automatic agent handoffs based on pending agent state
- Stream agent responses and display them to the user
- Parse handoff markers to determine which agent should respond next
- Maintain conversation flow across agent transitions

### 3. HANDOFF Marker Parsing (`parse_handoff()` method)
- Parse agent responses for lines containing `HANDOFF:<AgentName>`
- Extract the target agent name from the marker
- Map agent names to valid agent keys (coordinator, flight, hotel, activity, transfer, reference)
- Return the appropriate agent key or None if no valid handoff is found

### 4. Context-Aware Prompts (`build_prompt()` method)
- Retrieve recent conversation context using the provided `render_context()` helper
- Combine conversation history with the latest user request
- Add appropriate handoff instructions for non-coordinator agents
- Return a complete prompt that maintains conversation continuity

## Run the orchestrator

Once you've completed the TODO sections, test your implementation:

```bash
python travel_multi_agent_client.py
```

You will be greeted with a console prompt. Type natural language prompts ("Find me a flight from SEA to LHR next June") and the coordinator will decide when to hand over to the flight/hotel/activity/transfer agents. Agents emit `HANDOFF:<Agent>` markers to signal the next executor, closely matching the C# `AgentWorkflowBuilder.CreateHandoffBuilderWith(...)` behavior. Type `summary` to see collected trip snippets or `exit` to leave the session.

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

### Orchestration Patterns (C# Reference)

The C# Microsoft Agent Framework provides `AgentWorkflowBuilder` with four patterns:

1. **Sequential**: `AgentWorkflowBuilder.BuildSequential(agents)` - Agents execute in a fixed order
2. **Concurrent**: `AgentWorkflowBuilder.BuildConcurrent(agents)` - All agents run in parallel
3. **Handoff**: `AgentWorkflowBuilder.CreateHandoffBuilderWith(coord).WithHandoff(...)` - Dynamic delegation
4. **Agents-as-Tools**: `agent.AsAIFunction()` - Wrap agents as callable tools

The Python SDK currently exposes raw building blocks (ChatAgent, threads, tools) without the high-level workflow builder layer. This challenge focuses on implementing the **Handoff pattern** manually to demonstrate the core orchestration concepts.

### Handoff Pattern Details

In the handoff pattern:
- The **Coordinator** agent acts as the primary interface with users
- When specialized work is needed, the coordinator emits `HANDOFF:Flight` (or Hotel, Activity, etc.)
- The specialist agent performs its task and returns control with `HANDOFF:Coordinator`
- Your code must parse these markers and switch between agents accordingly
- Each agent maintains its own thread but shares conversation context

This pattern enables dynamic, context-driven workflows where agents collaborate naturally.

## Next Steps

After completing the core implementation:
- Experiment with the policy agent integration by setting up Azure AI Foundry credentials
- Consider how you might implement the other orchestration patterns (sequential, concurrent, agents-as-tools)
- Extend the agent specifications to add new capabilities or specialists
- Compare your implementation with the C# version to understand the architectural differences
