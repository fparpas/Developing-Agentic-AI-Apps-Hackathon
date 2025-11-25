"""Travel Multi-Agent Client (Python) - Challenge 08

CHALLENGE OBJECTIVES:
- Understand different agent orchestration patterns and when to use each
- Implement dynamic agent handoff based on conversation context
- Manage shared conversation state across multiple specialized agents
- Integrate persistent agents (policy checker) with ephemeral chat agents

ARCHITECTURE OVERVIEW:
The C# Microsoft Agent Framework provides AgentWorkflowBuilder with four orchestration patterns:
  1. Sequential: Agents execute in a fixed order (pipeline)
  2. Concurrent: All agents run in parallel (broadcast)
  3. Handoff: Coordinator dynamically delegates to specialists based on context
  4. Agents-as-Tools: One orchestrator calls specialists as callable functions

In C#, AgentWorkflowBuilder.CreateHandoffBuilderWith(...).WithHandoff(...) is part of
Microsoft's Agents/Workflow layer, which sits on top of the same SDK but adds orchestration
primitives, event streams, etc. The Python preview SDK currently exposes the raw building
blocks (chat client + agents + threads + tools) but does not yet surface the workflow
builder APIs, so we reproduce the pattern manually.

YOUR TASK:
Complete the TravelMultiAgentClient class to:
1. Initialize all specialist agents with proper tools and instructions
2. Implement the handoff workflow loop with agent switching logic
3. Parse HANDOFF: markers from agent responses to determine next agent
4. Build context-aware prompts that include conversation history
5. Maintain conversation state across agent handoffs
6. Integrate with the Azure AI Foundry policy agent for compliance checking

The agent specifications, helper utilities, and scaffolding are provided. Focus on
understanding the orchestration architecture and implementing the core workflow logic.
"""

# Only needed for Python versions < 3.11
from __future__ import annotations

import asyncio
import os
import sys
from contextlib import AsyncExitStack
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional

from agent_framework import ChatAgent, MCPStdioTool
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()

HANDOFF_PREFIX = "HANDOFF:"
MAX_TRANSCRIPT_TURNS = 40
MAX_SUMMARY_NOTES = 12


class AzureOpenAISettings:
    DEFAULT_API_VERSION = "2025-04-01-preview"

    def __init__(self, endpoint: str, api_key: str, deployment_name: str, api_version: str | None = None) -> None:
        self.endpoint = endpoint
        self.api_key = api_key
        self.deployment_name = deployment_name
        self.api_version = api_version or self.DEFAULT_API_VERSION

    @classmethod
    def from_env(cls) -> "AzureOpenAISettings":
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        version = os.getenv("AZURE_OPENAI_API_VERSION", cls.DEFAULT_API_VERSION)
        if not all([endpoint, api_key, deployment]):
            raise RuntimeError(
                "Set AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_API_KEY, and AZURE_OPENAI_DEPLOYMENT_NAME"
            )
        return cls(endpoint=endpoint, api_key=api_key, deployment_name=deployment, api_version=version)

    def to_client_kwargs(self) -> Dict[str, str]:
        return {
            "endpoint": self.endpoint,
            "api_key": self.api_key,
            "deployment_name": self.deployment_name,
            "api_version": self.api_version,
        }


@dataclass
class AgentSpec:
    key: str
    name: str
    instructions: str
    tools: List[Any] = field(default_factory=list)
    display_name: Optional[str] = None


def build_agent_specs(current_date: str, travel_tool: MCPStdioTool) -> List[AgentSpec]:
    """Return the ordered agent specs used throughout the workflow.

    ORCHESTRATION PATTERNS NOTE:
    In C#, Microsoft Agent Framework provides AgentWorkflowBuilder with:
    - Sequential workflow: AgentWorkflowBuilder.BuildSequential(agents)
    - Concurrent workflow: AgentWorkflowBuilder.BuildConcurrent(agents)
    - Handoff workflow: AgentWorkflowBuilder.CreateHandoffBuilderWith(coord).WithHandoff(...)
    - Agents-as-Tools: agent.AsAIFunction() to wrap agents as callable tools

    The Python SDK exposes ChatAgent, threads, and tools as primitives.
    This implementation demonstrates the Handoff pattern by manually coordinating
    agent transitions based on HANDOFF: markers in agent responses.
    """

    return [
        AgentSpec(
            key="coordinator",
            name="CoordinatorAgent",
            display_name="Coordinator",
            instructions=(
                "You are the Travel Coordinator Agent and act as the primary interface with"
                " customers. Understand requirements, plan trips, and decide when a"
                " specialist should respond. When you need help, emit a plain line that contains"
                " `HANDOFF:<Agent>` (Flight, Hotel, Activity, Transfer, Reference) with no"
                " additional markdown or prose. When you"
                " reclaim the conversation, output `HANDOFF:Coordinator`. Always greet"
                " warmly, keep the discussion organized, assume flexible budgets unless"
                " specified, and remind the user that bookings are not finalized inside this"
                " demo."
            ),
        ),
        AgentSpec(
            key="flight",
            name="FlightAgent",
            display_name="Flight Agent",
            tools=[travel_tool],
            instructions=(
                "You are a flight specialist. Search for routes, compare cabin classes,"
                " highlight prices, and use the `[FLIGHT] search_flight_offers` MCP tool for"
                " live data. Clarify travel dates, airports, passenger counts, and cabin"
                " class. Explain trade-offs (duration, stops, airlines) and finish every"
                " response with `HANDOFF:Coordinator`."
            ),
        ),
        AgentSpec(
            key="hotel",
            name="HotelAgent",
            display_name="Hotel Agent",
            tools=[travel_tool],
            instructions=(
                "You handle accommodations. Collect city or neighborhood preferences,"
                " budgets, star ratings, and amenities. Use the `[HOTEL] search_hotel_offers`"
                " MCP tool to fetch availability and pricing. Summarize the top matches with"
                " amenities and check-in/out reminders, then emit `HANDOFF:Coordinator`."
            ),
        ),
        AgentSpec(
            key="activity",
            name="ActivityAgent",
            display_name="Activity Agent",
            instructions=(
                "Recommend experiences, attractions, dining, and cultural highlights."
                " Consider timing, safety, and traveler profile. No MCP tools are available,"
                " so rely on reasoning and prior context. Close with `HANDOFF:Coordinator`."
            ),
        ),
        AgentSpec(
            key="transfer",
            name="TransferAgent",
            display_name="Transfer Agent",
            instructions=(
                "Plan airport transfers, local ground transportation, and chauffeur options."
                " Explain vehicle classes (standard, business, first, vans) and service"
                " levels (private, shared, hourly). Offer clear pickup instructions and"
                " pricing assumptions. End with `HANDOFF:Coordinator`."
            ),
        ),
        AgentSpec(
            key="reference",
            name="ReferenceAgent",
            display_name="Reference Agent",
            instructions=(
                "Provide travel reference data: airport codes, airline facts, destination"
                " stats, and safety considerations. Keep answers concise, data-driven, and"
                " conclude with `HANDOFF:Coordinator`."
            ),
        ),
    ]


def policy_agent_configured() -> bool:
    return bool(os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT") and os.getenv("AZURE_AI_AGENT_ID"))


async def run_policy_check(conversation_summary: str) -> str:
    endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
    agent_id = os.getenv("AZURE_AI_AGENT_ID")
    if not endpoint or not agent_id:
        return "Travel policy agent not configured."

    try:
        credential = DefaultAzureCredential()
        project_client = AIProjectClient(endpoint=endpoint, credential=credential)
        agents_client = project_client.agents
        agent = agents_client.get_agent(agent_id)

        thread = agents_client.threads.create()
        content_text = (
            "Check if this itinerary complies with our corporate travel policy."
            "\n\nConversation Summary:\n"
            f"{conversation_summary}"
        )
        agents_client.messages.create(
            thread_id=thread.id,
            role="user",
            content=[{"type": "text", "text": content_text}],
        )

        run = agents_client.runs.create(thread_id=thread.id, agent_id=agent.id)
        while run.status in {"queued", "in_progress"}:
            await asyncio.sleep(0.5)
            run = agents_client.runs.get(thread_id=thread.id, run_id=run.id)

        if run.status != "completed":
            return f"Policy agent run failed: {run.status}"

        messages = agents_client.messages.list(thread_id=thread.id, order="asc")
        reply = _extract_policy_reply(messages)
        if reply:
            return reply
        return "Policy agent completed with an empty response."
    except Exception as exc:  # pragma: no cover - depends on remote service
        details = _format_policy_error(exc)
        return f"Policy agent request failed: {details}"


def _extract_policy_reply(messages: Any) -> Optional[str]:
    data = getattr(messages, "data", None)
    if not data:
        data = list(messages)
    for entry in reversed(data):
        role = getattr(entry, "role", None)
        if role not in {"assistant", "tool"}:
            continue
        for content in getattr(entry, "content", []) or []:
            text_block = getattr(content, "text", None)
            if hasattr(text_block, "value"):
                value = getattr(text_block, "value")
            elif isinstance(text_block, dict):
                value = text_block.get("value")
            else:
                value = text_block
            if value:
                return value
    return None


def _format_policy_error(exc: Exception) -> str:
    response = getattr(exc, "response", None)
    status = getattr(response, "status_code", None)
    body = None
    if response is not None:
        json_body = getattr(response, "json", None)
        if callable(json_body):
            try:
                body = json_body()
            except Exception:
                pass
        if body is None:
            body = getattr(response, "text", None)
        if body is None:
            body = getattr(response, "body", None)
    status_text = f"status={status}" if status else "status=unknown"
    body_text = f" body={body}" if body else ""
    return f"{exc} ({status_text}{body_text})"



@dataclass
class ConversationTurn:
    speaker: str
    content: str


class TravelMultiAgentClient:
    def __init__(self) -> None:
        # TODO: Initialize instance variables for managing the multi-agent workflow:
        #
        # 1. self.exit_stack = AsyncExitStack() - for resource management
        # 2. self.agents: Dict[str, ChatAgent] = {} - store ChatAgent instances by key
        # 3. self.agent_threads: Dict[str, Any] = {} - maintain separate threads per agent
        # 4. self.agent_labels: Dict[str, str] = {} - display names for each agent
        # 5. self.current_agent_key = "coordinator" - track which agent is active
        # 6. self.pending_agent: Optional[str] = None - queue next agent for auto-handoff
        # 7. self.pending_prompt: Optional[str] = None - prompt for pending agent
        # 8. self.last_user_message: str = "" - cache latest user input
        # 9. self.transcript: List[ConversationTurn] = [] - conversation history
        # 10. self.trip_notes: List[str] = [] - collect specialist recommendations
        # 11. self.policy_available = policy_agent_configured() - check if policy agent exists
        pass

    async def initialize(self) -> None:
        # TODO: Complete the agent initialization workflow:
        #
        # STEP 1: Load Azure OpenAI configuration
        #   - Use AzureOpenAISettings.from_env() to load settings from environment
        #   - Create AzureOpenAIResponsesClient using settings.to_client_kwargs()
        #
        # STEP 2: Set up the Travel MCP Server connection
        #   - Get server path from TRAVEL_MCP_SERVER_PATH env var (default: "travel_mcp_server/server.py")
        #   - Validate the server file exists
        #   - Create MCPStdioTool with name="TravelMCP", command=sys.executable, args=[server_path]
        #
        # STEP 3: Build agent specifications
        #   - Get current date as string (YYYY-MM-DD format)
        #   - Call build_agent_specs(current_date, mcp_tool) to get agent definitions
        #
        # STEP 4: Create and register all agents
        #   - For each AgentSpec in the list:
        #     a) Create ChatAgent using await self.exit_stack.enter_async_context()
        #        Pass: chat_client, name=spec.name, instructions=spec.instructions, tools=spec.tools
        #     b) Store agent in self.agents[spec.key]
        #     c) Get a new thread: self.agent_threads[spec.key] = agent.get_new_thread()
        #     d) Store display name: self.agent_labels[spec.key] = spec.display_name or spec.name
        #
        # STEP 5: Set initial state
        #   - Set self.current_agent_key = "coordinator"
        #   - Print initialization success message
        #   - Check self.policy_available and print appropriate message
        #
        # HINT: Use async context manager pattern to ensure proper resource cleanup
        pass

    def record_turn(self, speaker: str, content: str) -> None:
        content = content.strip()
        if not content:
            return
        self.transcript.append(ConversationTurn(speaker=speaker, content=content))
        if len(self.transcript) > MAX_TRANSCRIPT_TURNS:
            self.transcript[:] = self.transcript[-MAX_TRANSCRIPT_TURNS:]

    def capture_trip_note(self, agent_key: str, text: str) -> None:
        if agent_key == "coordinator":
            return
        snippet = " ".join(text.strip().split())
        if not snippet:
            return
        label = self.agent_labels.get(agent_key, agent_key.title())
        self.trip_notes.append(f"{label}: {snippet[:200]}")
        if len(self.trip_notes) > MAX_SUMMARY_NOTES:
            self.trip_notes[:] = self.trip_notes[-MAX_SUMMARY_NOTES:]

    def render_context(self, limit: int = 6) -> str:
        recent = self.transcript[-limit:]
        return "\n".join(f"{turn.speaker}: {turn.content}" for turn in recent)

    def build_prompt(self, agent_key: str, latest_input: str) -> str:
        # TODO: Build a context-aware prompt for the agent:
        #
        # 1. Get recent conversation context using self.render_context()
        # 2. Create a list of prompt pieces:
        #    - If context exists, add "Conversation so far:\n" + context
        #    - Add "Latest user request:\n" + latest_input
        #    - For non-coordinator agents, add handoff instruction:
        #      "If you are done, place `HANDOFF:Coordinator` on its own line so the coordinator resumes the conversation."
        # 3. Join all pieces with "\n\n" separator and return
        #
        # This ensures each agent has conversation context to maintain continuity
        pass

    def build_policy_summary(self) -> str:
        if self.trip_notes:
            return " | ".join(self.trip_notes)
        return self.render_context(limit=8)

    def parse_handoff(self, response_text: str) -> Optional[str]:
        # TODO: Parse agent responses for HANDOFF: markers to determine next agent
        #
        # The handoff workflow relies on agents explicitly stating which agent should
        # respond next by including a line like "HANDOFF:Flight" or "HANDOFF:Coordinator"
        #
        # Implementation steps:
        # 1. Split response_text into individual lines
        # 2. For each line:
        #    a) Strip whitespace and convert to uppercase for comparison
        #    b) Check if line contains HANDOFF_PREFIX ("HANDOFF:")
        #    c) If found, extract the text after the colon
        #    d) Clean the target name (remove special chars, convert to lowercase)
        #    e) Map to valid agent keys using a dictionary:
        #       {"coordinator": "coordinator", "flight": "flight", "hotel": "hotel",
        #        "activity": "activity", "transfer": "transfer", "reference": "reference"}
        #    f) If target matches a valid agent, return the mapped key
        # 3. If no valid handoff found, return None
        #
        # HINT: Use str.find() or "in" operator to locate HANDOFF_PREFIX
        pass

    async def run(self) -> None:
        print("=" * 70)
        print("Travel Multi-Agent Client (Python)")
        print("Commands: summary, policy, exit")
        print("=" * 70 + "\n")

        while True:
            try:
                # TODO: Implement the handoff workflow pattern
                #
                # This is the core orchestration loop that coordinates multiple agents.
                #
                # KEY STEPS:
                # 1. Check if there's a pending agent handoff (self.pending_agent)
                #    - If yes, use that agent automatically and clear the pending state
                #    - If no, prompt the user for input
                #
                # 2. Handle special commands: "exit", "summary", "policy"
                #    - exit: break the loop
                #    - summary: print self.trip_notes if available
                #    - policy: call run_policy_check() with conversation summary
                #
                # 3. Get the active agent and its thread from your dictionaries
                #
                # 4. Build the prompt using self.build_prompt(agent_key, user_input)
                #
                # 5. Stream the agent's response:
                #    - Use: async for update in agent.run_stream(prompt, thread=thread)
                #    - Print each chunk and accumulate the full response
                #
                # 6. Record the conversation turn and capture notes for the summary
                #
                # 7. Parse for handoff using self.parse_handoff(response_text)
                #    - If handoff found, update self.current_agent_key and self.pending_agent
                #    - Print a handoff message to show which agent is next
                #
                # HINT: Look at the C# Program.cs StartInteractiveChat(Workflow workflow) method
                # for reference on handling the workflow pattern.

                pass

            except KeyboardInterrupt:
                print("\nSession interrupted. Goodbye!\n")
                break
            except Exception as exc:
                print(f"\n[ERROR] {exc}\n")

    async def close(self) -> None:
        await self.exit_stack.aclose()


def ensure_travel_server_exists() -> None:
    server_path = Path(os.getenv("TRAVEL_MCP_SERVER_PATH", "travel_mcp_server/server.py"))
    if not server_path.exists():
        raise FileNotFoundError(
            f"Cannot locate {server_path}. Copy the travel_mcp_server folder or update TRAVEL_MCP_SERVER_PATH."
        )


async def main() -> None:
    ensure_travel_server_exists()
    client = TravelMultiAgentClient()
    try:
        await client.initialize()
        await client.run()
    finally:
        await client.close()


if __name__ == "__main__":
    asyncio.run(main())
