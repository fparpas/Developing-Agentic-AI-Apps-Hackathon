# Challenge 08 Solution - Multi-Agent Travel Planning Assistant (Python)

## Overview

This solution demonstrates **multi-agent orchestration** using Microsoft Agent Framework. It implements a **Handoff Workflow Pattern** where specialized agents collaborate to plan comprehensive travel itineraries.

## Architecture

### Orchestration Pattern: Handoff Workflow

The **Handoff Workflow** is the optimal pattern for travel planning because:

✅ **Multi-Turn Conversational** - Travel planning is iterative and conversational
✅ **Context-Aware Routing** - Dynamically routes to appropriate specialists
✅ **Natural Interaction** - Mimics real-world travel agency experiences
✅ **Flexible and Adaptive** - No predetermined execution order
✅ **Resource Efficient** - Only activates agents when needed

```
┌─────────────────────────────────────────────┐
│              User Interface                 │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
      ┌────────────────────────┐
      │  CoordinatorAgent      │◄────┐
      │  (Entry Point)         │     │
      └───────┬────────────────┘     │
              │                      │
     ┌────────┼────────┐             │
     │        │        │             │
     ▼        ▼        ▼             │
┌────────┐ ┌────────┐ ┌────────┐     │
│Flight  │ │Hotel   │ │Activity│─────┘
│Agent   │ │Agent   │ │Agent   │
└────┬───┘ └────┬───┘ └────┬───┘
     │          │          │
     └──────────┴──────────┘
     Handoff back to Coordinator
```

### Specialized Agents

| Agent | Role | Capabilities |
|-------|------|--------------|
| **CoordinatorAgent** | Main interface & orchestrator | Routes requests, synthesizes results, provides summaries |
| **FlightAgent** | Flight search specialist | Uses the `[FLIGHT] search_flight_offers` MCP tool to fetch real itineraries |
| **HotelAgent** | Accommodation specialist | Calls `[HOTEL] search_hotel_offers` for live pricing and availability |
| **ActivityAgent** | Local attractions expert | Provides recommendations via reasoning (no MCP tool) |
| **TransferAgent** | Ground transportation | Shares transport guidance via reasoning (no MCP tool) |
| **ReferenceAgent** | Reference data provider | Airport codes, time zones, location information |
| **TravelPolicyAgent** | Policy compliance (optional) | Validates trips against company policies (Azure AI Foundry) |

## How It Works

### Handoff Workflow Pattern

The system uses **intelligent handoffs** between agents:

1. **User starts with CoordinatorAgent** - The coordinator understands travel needs
2. **Dynamic routing** - Based on context, coordinator hands off to specialists:
   - "I need a flight" → Handoff to **FlightAgent**
   - "What hotels are available?" → Handoff to **HotelAgent**
   - "Things to do in Paris?" → Handoff to **ActivityAgent**
3. **Agents hand back** - Specialists return control to coordinator
4. **Context preserved** - Conversation history maintained across handoffs
5. **Iterative refinement** - Users can refine choices, revisit agents

### Example Conversation Flow

```
User: "I want to travel to Paris next month"
→ CoordinatorAgent: Greets, gathers requirements

User: "Show me flights from New York on June 15"
→ [HANDOFF] FlightAgent: Searches, presents options
→ [HANDOFF BACK] CoordinatorAgent

User: "I'll take the morning flight. Need a hotel near Eiffel Tower"
→ [HANDOFF] HotelAgent: Searches hotels in that area
→ [HANDOFF BACK] CoordinatorAgent

User: "What can I do there?"
→ [HANDOFF] ActivityAgent: Recommends attractions
→ [HANDOFF BACK] CoordinatorAgent

User: "summary"
→ CoordinatorAgent: Provides complete itinerary
```

## Prerequisites

1. **Azure OpenAI** resource with a deployed model (e.g., `gpt-4o`)
2. **Python 3.11+**
3. **Amadeus API credentials** - Register at [Amadeus for Developers](https://developers.amadeus.com/) for real flight and hotel data
4. **(Optional)** Azure AI Foundry project with a persistent Travel Policy Agent from Challenge-05

## Setup Instructions

### 1. Install Dependencies

```bash
# Create Python Virtual Environment
cd Coach/Solutions/Challenge-08/python
python -m venv .venv # or uv venv
source .venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt # or uv pip install -r requirements.txt
```

### 2. Configure Environment

Copy the example environment file:

```bash
cp .env.example .env
```

Edit `.env` with your Azure credentials:

```env
# Required - Azure OpenAI
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-api-key-here
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o
AZURE_OPENAI_API_VERSION=2024-08-01-preview

# Required - Amadeus Travel API
AMADEUS_API_KEY=your-amadeus-api-key-here
AMADEUS_API_SECRET=your-amadeus-api-secret-here

# Optional (for Policy Agent)
AZURE_AI_FOUNDRY_PROJECT_ENDPOINT=https://your-project.api.azureml.ms
AZURE_AI_AGENT_ID=your-agent-id-here
```

### 3. Run the Application

```bash
python multi_agent_travel_planner.py
```

### 4. (Optional) Sanity-Test the Data Path

Use the lightweight harness to exercise both the raw Amadeus SDK and the MCP
server with the same query before launching the orchestrator:

```bash
python test_amadeus_vs_mcp.py
```

The script prints two blocks so you can compare the direct SDK response with the
`search_flight_offers` tool output.

## Usage

### Interactive Commands

- **Normal conversation** - Ask about flights, hotels, activities, etc.
- **`summary`** - Display accumulated trip details
- **`policy`** - Check trip against company policy (if configured)
- **`exit`** or **`quit`** - End session

### Example Queries

```
"I need to fly from Seattle to Tokyo in December"
"Find hotels near Shibuya station under $200/night"
"What are the must-see attractions in Tokyo?"
"How do I get from Narita Airport to my hotel?"
"What's the airport code for Tokyo?"
```

## Key Features

### Real Travel Data via Amadeus API

This solution uses **real flight and hotel data** from the Amadeus Travel API via an MCP server:

- **FlightAgent** - Real-time flight search, pricing, and availability
- **HotelAgent** - Actual hotel search and offers with live pricing
- **MCP Integration** - Travel data accessed through Model Context Protocol tools

The `travel_mcp_server` provides Amadeus API integration using the official Python SDK:
- **Official Amadeus SDK** (`amadeus>=8.1.0`) - Reliable, maintained API client
- `[FLIGHT] search_flight_offers` – Returns up to five itineraries with price summaries (accepts either city names or IATA codes)
- `[HOTEL] search_hotel_offers` – Lists hotel options for a city/date range (auto-resolves common city names to codes)
- Automatic authentication and error handling

### Intelligent Handoffs

The system automatically detects when to hand off to specialized agents based on:
- Keywords in user input (flight, hotel, activity, etc.)
- Context from agent responses
- Natural conversation flow

### Bidirectional Communication

- Coordinator → Specialist (forward handoff)
- Specialist → Coordinator (return handoff)
- Maintains conversation continuity

### Trip Summary

Use the `summary` command to see accumulated trip details from all agents.

### Policy Compliance (Optional)

If configured with Azure AI Foundry agent:
- Type `policy` to validate trip against company policies
- Uses persistent agent from Challenge-05
- Provides compliance feedback

## Code Structure

```python
class TravelAgentOrchestrator:
    """Main orchestrator implementing handoff workflow."""

    async def initialize_agents(self):
        """Create all specialized agents with specific instructions."""

    async def run_handoff_workflow(self):
        """Execute interactive handoff workflow."""

    def parse_handoff_request(self, response: str):
        """Detect handoff requests from context."""

    async def check_travel_policy(self, trip_details: str):
        """Validate against Azure AI Foundry policy agent."""
```

## Educational Notes

### Why Handoff Pattern?

This solution uses the **Handoff Workflow** because travel planning is:

1. **Conversational** - Users iteratively refine choices
2. **Non-linear** - Order of operations varies by user
3. **Contextual** - Decisions depend on previous choices
4. **Flexible** - Not all users need all services

### Alternative Patterns (Not Used Here)

**Sequential Workflow** - Too rigid; forces all steps in order
**Concurrent Workflow** - Can't handle dependencies between agents
**Agents as Tools** - Works, but less natural for multi-turn conversations

See the C# README for detailed pattern comparison.

## Design Principles

### Separation of Concerns
Each agent has a **single, well-defined responsibility**.

### Agent Instructions
Each agent has **specialized system instructions** tailored to its role.

### Loose Coupling
Agents communicate through the coordinator, not directly with each other.

### Context Preservation
Conversation history is maintained across handoffs.

### Lazy Activation
Agents are only invoked when needed, not pre-emptively.

## Extending the Solution

### Add New Agents

1. Create new `ChatAgent` in `initialize_agents()`
2. Add specialized instructions
3. Update handoff detection in `parse_handoff_request()`
4. Add to agent mapping in `get_agent_by_type()`

### Example: Adding a Weather Agent

```python
self.weather_agent = await self.exit_stack.enter_async_context(
    ChatAgent(
        chat_client=chat_client,
        name="WeatherAgent",
        instructions="You are a weather specialist. Provide forecasts and travel weather advice."
    )
)
```

Then update detection:
```python
handoff_patterns = {
    # ... existing patterns ...
    'weather': ['weather', 'forecast', 'temperature', 'climate']
}
```

## Troubleshooting

### Issue: Agents not handing off

**Solution:** Check that keywords in `parse_handoff_request()` match your conversation patterns. The system uses keyword detection for simplicity in this educational example.

### Issue: Policy agent not working

**Solution:** Ensure:
1. Azure AI Foundry endpoint is correct
2. Agent ID is valid
3. You have proper Azure credentials configured
4. The persistent agent exists in your Azure AI Foundry project

### Issue: API rate limits

**Solution:** Add delays between requests or upgrade your Azure OpenAI tier.

## Learning Outcomes

After completing this solution, you should understand:

- Multi-agent orchestration patterns
- Handoff workflow implementation
- Agent specialization and role design
- Context management in conversations
- Integration with Azure AI Foundry persistent agents
- When to use different orchestration patterns

## Additional Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [AI Agent Orchestration Patterns](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns)
- [Workflows and Orchestrations](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/overview)

## Success Criteria

- [x] Created 6+ specialized agents with distinct roles
- [x] Implemented Handoff Workflow pattern
- [x] Coordinator orchestrates agent collaboration
- [x] Dynamic routing based on conversation context
- [x] **Real travel data from Amadeus API**
- [x] **MCP server integration for flight and hotel tools**
- [x] Travel planning functionality (flights, hotels, activities, transfers)
- [x] Optional policy compliance integration
- [x] Clean, educational code structure
- [x] Comprehensive documentation

---

## Architecture

```
┌─────────────────────────────────────┐
│   Multi-Agent Travel Planner        │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  FlightAgent + HotelAgent   │   │
│  │  (with MCP tools)           │   │
│  └──────────┬──────────────────┘   │
│             │                       │
└─────────────┼───────────────────────┘
              │ MCP Protocol
              ▼
┌─────────────────────────────────────┐
│     Travel MCP Server                │
│                                     │
│  ┌─────────────────────────────┐   │
│  │  Amadeus Auth Service       │   │
│  ├─────────────────────────────┤   │
│  │  Flight Tools               │   │
│  │  - search_flight_offers     │   │
│  ├─────────────────────────────┤   │
│  │  Hotel Tools                │   │
│  │  - search_hotel_offers      │   │
│  └──────────┬──────────────────┘   │
└─────────────┼───────────────────────┘
              │ HTTPS
              ▼
┌─────────────────────────────────────┐
│     Amadeus Travel API              │
│     (Real flight & hotel data)      │
└─────────────────────────────────────┘
```

**Note:** This solution demonstrates production-ready orchestration patterns with real travel APIs. For additional production features, consider adding:
- Enhanced error handling and retries
- Comprehensive logging and telemetry
- State persistence and session management
- More sophisticated handoff logic
- User authentication and authorization
- Cost tracking and budget management
- Caching for frequently accessed data
