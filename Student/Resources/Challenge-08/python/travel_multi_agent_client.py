"""Travel Multi-Agent Client (Python)

Using Microsoft Agent Framework Python SDK. The coordinator chats with the user,
then hands work to specialists that optionally call MCP tools backed by the Amadeus API.

In C#, AgentWorkflowBuilder.CreateHandoffBuilderWith(...).WithHandoff(...) is part of
Microsoft’s Agents/Workflow layer, which sits on top of the same SDK but adds orchestration
primitives, event streams, etc. The Python preview SDK currently exposes the raw building
blocks (chat client + agents + threads + tools) but does not yet surface the workflow
builder APIs, so we reproduce the pattern manually.

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
    """Return the ordered agent specs used throughout the workflow."""

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
        self.exit_stack = AsyncExitStack()
        self.agents: Dict[str, ChatAgent] = {}
        self.agent_threads: Dict[str, Any] = {}
        self.agent_labels: Dict[str, str] = {}
        self.current_agent_key = "coordinator"
        self.pending_agent: Optional[str] = None
        self.pending_prompt: Optional[str] = None
        self.last_user_message: str = ""
        self.transcript: List[ConversationTurn] = []
        self.trip_notes: List[str] = []
        self.policy_available = policy_agent_configured()

    async def initialize(self) -> None:
        settings = AzureOpenAISettings.from_env()
        chat_client = AzureOpenAIResponsesClient(**settings.to_client_kwargs())

        server_path = Path(os.getenv("TRAVEL_MCP_SERVER_PATH", "travel_mcp_server/server.py"))
        if not server_path.exists():
            raise FileNotFoundError(
                f"Travel MCP server not found at {server_path}. Set TRAVEL_MCP_SERVER_PATH or copy the server folder."
            )

        mcp_tool = MCPStdioTool(name="TravelMCP", command=sys.executable, args=[str(server_path.resolve())])
        today = datetime.utcnow().strftime("%Y-%m-%d")
        specs = build_agent_specs(today, mcp_tool)

        for spec in specs:
            agent = await self.exit_stack.enter_async_context(
                ChatAgent(chat_client=chat_client, name=spec.name, instructions=spec.instructions, tools=spec.tools)
            )
            self.agents[spec.key] = agent
            self.agent_threads[spec.key] = agent.get_new_thread()
            self.agent_labels[spec.key] = spec.display_name or spec.name

        self.current_agent_key = "coordinator"
        print("[OK] Agents ready. Coordinator is the entry point.\n")
        if self.policy_available:
            print("[OK] Travel policy agent configured. Use the 'policy' command to invoke it.\n")
        else:
            print("[INFO] Travel policy agent not configured (optional).\n")

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
        context = self.render_context()
        pieces = []
        if context:
            pieces.append("Conversation so far:\n" + context)
        pieces.append("Latest user request:\n" + latest_input)
        if agent_key != "coordinator":
            pieces.append(
                "If you are done, place `HANDOFF:Coordinator` on its own line so the"
                " coordinator resumes the conversation."
            )
        return "\n\n".join(pieces)

    def build_policy_summary(self) -> str:
        if self.trip_notes:
            return " | ".join(self.trip_notes)
        return self.render_context(limit=8)

    def parse_handoff(self, response_text: str) -> Optional[str]:
        for line in response_text.splitlines():
            stripped = line.strip()
            upper_line = stripped.upper()
            index = upper_line.find(HANDOFF_PREFIX)
            if index == -1:
                continue
            target_part = stripped[index + len(HANDOFF_PREFIX) :]
            target = target_part.strip(" *`_-\t").lower()
            mapping = {
                "coordinator": "coordinator",
                "flight": "flight",
                "hotel": "hotel",
                "activity": "activity",
                "transfer": "transfer",
                "reference": "reference",
            }
            if target in mapping:
                return mapping[target]
        return None

    async def run(self) -> None:
        print("=" * 70)
        print("Travel Multi-Agent Client (Python)")
        print("Commands: summary, policy, exit")
        print("=" * 70 + "\n")

        while True:
            try:
                if self.pending_agent:
                    agent_key = self.pending_agent
                    user_input = self.pending_prompt or self.last_user_message or "Continue assisting the user."
                    self.pending_agent = None
                    self.pending_prompt = None
                    auto_mode = True
                else:
                    agent_key = self.current_agent_key
                    label = self.agent_labels.get(agent_key, agent_key.title())
                    user_input = input(f"You [{label}]: ").strip()
                    auto_mode = False
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
                            print("\n[Trip Summary] No specialist notes yet.\n")
                        continue
                    if lowered == "policy":
                        conversation_summary = self.build_policy_summary()
                        if not conversation_summary:
                            print("\nAdd some trip details before running a policy check.\n")
                            continue
                        if not policy_agent_configured():
                            self.policy_available = False
                            print("\nTravel policy agent not configured.\n")
                            continue
                        self.policy_available = True
                        print("\n[Policy] Checking itinerary...\n")
                        result = await run_policy_check(conversation_summary)
                        print(result + "\n")
                        continue

                    self.last_user_message = user_input
                    self.record_turn("User", user_input)

                label = self.agent_labels.get(agent_key, agent_key.title())
                agent = self.agents[agent_key]
                prompt = self.build_prompt(agent_key, user_input)
                thread = self.agent_threads[agent_key]

                if not auto_mode:
                    print()
                print(f"{label}: ", end="", flush=True)
                response_text = ""
                async for update in agent.run_stream(prompt, thread=thread):
                    chunk = update.text or ""
                    print(chunk, end="", flush=True)
                    response_text += chunk
                print("\n")

                self.record_turn(label, response_text)
                self.capture_trip_note(agent_key, response_text)

                handoff_target = self.parse_handoff(response_text)
                if handoff_target:
                    if handoff_target == "coordinator":
                        self.current_agent_key = "coordinator"
                        print("[Handoff → Coordinator]\n")
                    else:
                        self.current_agent_key = handoff_target
                        self.pending_agent = handoff_target
                        self.pending_prompt = response_text or self.last_user_message
                        next_label = self.agent_labels.get(handoff_target, handoff_target.title())
                        print(f"[Handoff → {next_label}]\n")
                else:
                    self.current_agent_key = "coordinator"

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
