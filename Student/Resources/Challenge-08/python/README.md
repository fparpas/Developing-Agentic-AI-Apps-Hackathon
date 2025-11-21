# Challenge 08 (Python) – Travel Multi-Agent Client

This folder mirrors the C# `MAF.TravelMultiAgentClient` sample with a lightweight Python port.  It uses the Microsoft Agent Framework to spin up the same specialist agents (coordinator, flight, hotel, activity, transfer, reference, and an optional travel-policy agent) and orchestrates them with a simple handoff workflow.  Flight and hotel data is provided through the Travel MCP server that wraps the Amadeus SDK.

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

## Run the orchestrator

```bash
python travel_multi_agent_client.py
```

You will be greeted with a console prompt.  Type natural language prompts ("Find me a flight from OTP to SEA next June") and the coordinator will decide when to hand over to the flight/hotel/activity/transfer agents.  Agents emit `HANDOFF:<Agent>` markers to signal the next executor, closely matching the C# `AgentWorkflowBuilder.CreateHandoffBuilderWith(...)` behavior.  Type `summary` to see collected trip snippets or `exit` to leave the session.

If `AZURE_AI_FOUNDRY_PROJECT_ENDPOINT` and `AZURE_AI_AGENT_ID` are set, the `policy` command sends the aggregated trip notes to your persistent agent for validation using `DefaultAzureCredential` (so `az login`, managed identity, VS Code identity, etc. are automatically honored).

## Travel MCP server

The embedded server under `travel_mcp_server/` is a trimmed copy of the coach solution.  It exposes two tools:

- `[FLIGHT] search_flight_offers`
- `[HOTEL] search_hotel_offers`

Both rely on the Amadeus SDK, so valid credentials are required.  The server can also be run manually for debugging:

```bash
cd travel_mcp_server
python server.py
```

## Next steps

- Extend `AGENT_SPECS` in `travel_multi_agent_client.py` to add new specialists.
- Implement other orchestration patterns (sequential, concurrent, agents-as-tools) following the C# sample if you want to experiment further.
- Connect to an Azure AI Foundry Persistent Agent from Challenge 05 to enforce travel policies inside the same workflow.
