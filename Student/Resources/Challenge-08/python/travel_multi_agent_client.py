"""
Student Solution

Multi-Agent Travel Planning Assistant - Challenge 08 (Python).

Uses HandoffBuilder from agent-framework-orchestrations to orchestrate
specialist agents. The framework manages agent routing via tool calls,
eliminating manual HANDOFF: marker parsing.

Agents are created with chat_client.as_agent() and connected through a
HandoffBuilder workflow. The builder injects handoff tools into each agent
so they can transfer control to one another automatically. Context is
broadcast to all participants after every turn.
"""

# Only necessary for Python versions prior to 3.11
from __future__ import annotations

import asyncio
import os
import sys
from datetime import datetime
from pathlib import Path
from typing import Any, List, Optional

from agent_framework import MCPStdioTool, WorkflowEvent
from agent_framework.azure import AzureOpenAIResponsesClient
from agent_framework.orchestrations import HandoffBuilder, HandoffAgentUserRequest
from azure.ai.agents import AgentsClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()

MAX_TRIP_NOTES = 12


class TravelAgentOrchestrator:
    """Coordinates specialist agents using HandoffBuilder orchestration."""

    def __init__(self) -> None:
        self.workflow = None
        self.pending_requests: List[WorkflowEvent] = []
        self.trip_notes: List[str] = []
        self.agents_client: Optional[AgentsClient] = None
        self.policy_agent_id: Optional[str] = None
        self.first_turn = True
        self.active_agent: Optional[str] = None

    async def initialize_agents(self) -> None:
        """Create agents via chat_client.as_agent() and build the HandoffBuilder workflow."""

        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2025-10-01-preview")

        if not all([endpoint, api_key, deployment]):
            raise ValueError(
                "Set AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, and AZURE_OPENAI_DEPLOYMENT_NAME"
            )

        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=api_key,
            deployment_name=deployment,
            api_version=api_version,
        )

        travel_server = Path(__file__).parent / "travel_mcp_server" / "server.py"
        if not travel_server.exists():
            raise FileNotFoundError(
                "travel_mcp_server/server.py not found. Create the MCP server first."
            )

        travel_mcp_tool = MCPStdioTool(
            name="TravelMCP",
            command=sys.executable,
            args=[str(travel_server)],
        )

        today = datetime.now()
        current_date = today.strftime("%Y-%m-%d")
        current_year = today.year

        # Create agents via chat_client.as_agent()

        coordinator = chat_client.as_agent(
            name="CoordinatorAgent",
            description="Travel coordinator that greets users and delegates to specialists.",
            instructions=(
                "You are the Travel Coordinator Agent and the primary interface with "
                "customers. Understand requirements, plan trips, and decide when a "
                "specialist should handle a request. Greet warmly, keep the discussion "
                "organised, and assume flexible budgets unless specified. "
                "IMPORTANT: You have NO booking tools. When the user confirms they want "
                "to book, present a friendly confirmation summary as if the booking was "
                "completed successfully, including a fake confirmation number. Do NOT "
                "attempt to call any booking function. Always output plain text, not Markdown."
            ),
        )

        reference = chat_client.as_agent(
            name="ReferenceAgent",
            description="Provides airport codes, city facts, and timezone reference data.",
            instructions=(
                f"Provide airport, city, and time-zone reference data. Today is {current_date}. "
                f"Always emit dates in YYYY-MM-DD with a four-digit year."
            ),
        )

        flight = chat_client.as_agent(
            name="FlightAgent",
            description="Searches for flight options using the Amadeus API via MCP.",
            instructions=(
                f"You are a flight specialist with access to a search_flight_offers MCP tool "
                f"backed by Amadeus. It expects origin/destination IATA codes, departure_date "
                f"(and optional return_date) in YYYY-MM-DD using year {current_year}, adults "
                f"passenger count, and optional travel_class. The tool returns plain-text "
                f"itinerary lines. Interpret relative dates before calling the tool and keep "
                f"results concise. "
                f"IMPORTANT: You can only SEARCH for flights. You have NO booking tools. "
                f"When the user confirms a selection, present a confirmation summary as if "
                f"the booking was completed (include a fake confirmation number). Do NOT "
                f"attempt to call any booking or reservation function. Always output plain text, not Markdown."
            ),
            tools=[travel_mcp_tool],
        )

        hotel = chat_client.as_agent(
            name="HotelAgent",
            description="Searches for hotel options using the Amadeus API via MCP.",
            instructions=(
                f"You search for accommodation using the search_hotel_offers MCP tool. "
                f"Provide city_code (IATA-style city), check_in_date/check_out_date "
                f"(YYYY-MM-DD), and adults count. Fill missing years with {current_year}. "
                f"Summarise the best matches. "
                f"IMPORTANT: You can only SEARCH for hotels. You have NO booking tools. "
                f"When the user confirms a selection, present a confirmation summary as if "
                f"the booking was completed (include a fake confirmation number). Do NOT "
                f"attempt to call any booking or reservation function. Always output plain text, not Markdown."
            ),
            tools=[travel_mcp_tool],
        )

        activity = chat_client.as_agent(
            name="ActivityAgent",
            description="Recommends activities, tours, and dining.",
            instructions=(
                "Recommend experiences, attractions, dining, and cultural highlights. "
                "No MCP tools are available - rely on reasoning and conversation context."
            ),
        )

        transfer = chat_client.as_agent(
            name="TransferAgent",
            description="Plans airport transfers and ground transportation.",
            instructions=(
                "Advise on airport transfers, ride services, and public transport. "
                "Explain vehicle classes and service levels. No MCP tools available."
            ),
        )

        # TODO: You need to implement this part to build the handoff workflow using HandoffBuilder.
        # The workflow should route between the agents based on the conversation context and user requests.
        # The HandoffBuilder will automatically inject handoff tools into each agent,
        # so they can transfer control to one another.
        #
        # If you get stuck, refer to the solution in the Coach directory.

        print("[OH NOES] You need to implement the HandoffBuilder workflow before running this script.");
        sys.exit(255);

        participants = [...]

        self.workflow = (
               ...
        )

        # Policy Agent (Called from Microsoft Foundry Agent Service
        foundry_endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
        agent_id = os.getenv("AZURE_AI_AGENT_ID")
        if foundry_endpoint and agent_id:
            self.agents_client = AgentsClient(
                endpoint=foundry_endpoint,
                credential=DefaultAzureCredential(),
            )
            self.policy_agent_id = agent_id
            print("[OK] Travel Policy Agent (Azure AI Foundry) configured")
        else:
            print("[INFO] Travel Policy Agent not configured (optional)")

        print("[OK] All agents initialized via HandoffBuilder\n")

    async def check_travel_policy(self, trip_summary: str) -> Optional[str]:
        """Validate trip details against a persistent policy agent."""

        if not self.agents_client or not self.policy_agent_id:
            return None

        try:
            thread = self.agents_client.threads.create()
            self.agents_client.messages.create(
                thread_id=thread.id,
                role="user",
                content=f"Check if this itinerary complies with policy: {trip_summary}",
            )

            run = self.agents_client.runs.create_and_process(
                thread_id=thread.id,
                agent_id=self.policy_agent_id,
            )

            if run.status == "completed":
                last_msg = self.agents_client.messages.get_last_message_text_by_role(
                    thread_id=thread.id, role="assistant"
                )
                if last_msg:
                    return last_msg.text.value
            return "Policy check completed (no additional details returned)."
        except Exception as exc:
            return f"Policy check unavailable: {exc}"

    def capture_trip_note(self, author: str, text: str) -> None:
        """Store agent responses for the summary / policy check."""

        if "coordinator" in author.lower():
            return
        snippet = " ".join(text.strip().split())
        if snippet:
            self.trip_notes.append(f"{author}: {snippet}")
            if len(self.trip_notes) > MAX_TRIP_NOTES:
                self.trip_notes = self.trip_notes[-MAX_TRIP_NOTES:]

    async def process_events(self, event_stream) -> None:
        """Consume workflow events, print agent responses, and collect pending requests."""

        self.pending_requests = []
        async for event in event_stream:
            # Detect handoffs from streaming output events
            if event.type == "output" and hasattr(event.data, "author_name"):
                author = event.data.author_name
                if author and author != self.active_agent:
                    if self.active_agent is not None:
                        print(f"\n[HANDOFF] {self.active_agent} --> {author}\n")
                    self.active_agent = author

            if event.type == "request_info" and isinstance(event.data, HandoffAgentUserRequest):
                self.pending_requests.append(event)
                for msg in event.data.agent_response.messages:
                    author = getattr(msg, "author_name", "Agent")
                    text = getattr(msg, "text", "")
                    if text:
                        print(f"\n{author}: {text}")
                        self.capture_trip_note(author, text)

    async def run_handoff_workflow(self) -> None:
        """Interactive console session using HandoffBuilder orchestration."""

        print("=" * 70)
        print("Multi-Agent Travel Planning Assistant (Python)")
        print("Using HandoffBuilder orchestration")
        print("=" * 70)
        print("Type 'summary' for trip notes, 'policy' to validate, 'exit' to quit.\n")

        while True:
            try:
                user_input = input("\nYou: ").strip()
                if not user_input:
                    continue

                lowered = user_input.lower()
                if lowered in {"exit", "quit"}:
                    print("\nThanks for planning with the travel assistant!")
                    break

                if lowered == "summary":
                    if self.trip_notes:
                        print("\n[Trip Summary]\n" + "\n".join(self.trip_notes) + "\n")
                    else:
                        print("\n[Trip Summary] No specialist guidance recorded yet.\n")
                    continue

                if lowered == "policy":
                    if not self.trip_notes:
                        print("\nAdd some trip details before running a policy check.\n")
                        continue
                    print("\n[Policy Check] Running...")
                    result = await self.check_travel_policy(" | ".join(self.trip_notes))
                    print(f"{result or 'Policy check unavailable.'}\n")
                    continue

                # First turn: start the workflow; subsequent turns: respond to
                # the pending request_info events from the previous round.
                if self.first_turn:
                    await self.process_events(self.workflow.run(user_input, stream=True))
                    self.first_turn = False
                else:
                    responses = {
                        req.request_id: HandoffAgentUserRequest.create_response(user_input)
                        for req in self.pending_requests
                    }
                    await self.process_events(self.workflow.run(responses=responses, stream=True))

            except KeyboardInterrupt:
                print("\nSession interrupted. Goodbye!\n")
                break
            except Exception as exc:
                print(f"\n[ERROR] {exc}\n")


def ensure_travel_server_available() -> None:
    """Fail fast if the MCP server script is missing."""

    server_path = Path(__file__).parent / "travel_mcp_server" / "server.py"
    if not server_path.exists():
        raise FileNotFoundError(
            "travel_mcp_server/server.py is required. Create it before running the orchestrator."
        )


async def main() -> None:
    ensure_travel_server_available()
    orchestrator = TravelAgentOrchestrator()
    try:
        print("Initializing travel agents...\n")
        await orchestrator.initialize_agents()
        await orchestrator.run_handoff_workflow()
    except Exception as exc:
        print(f"\n[FATAL] {exc}")
        import traceback

        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    asyncio.run(main())