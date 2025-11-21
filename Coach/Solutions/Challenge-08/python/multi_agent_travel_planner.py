"""Multi-Agent Travel Planning Assistant - Challenge 08 (Python).

Mirrors the C# handoff workflow while staying Pythonic. A coordinator agent and
specialists cooperate via Microsoft Agent Framework, delegating to an Amadeus
backed MCP server for live travel data. Agents reuse threads and exchange
context summaries so handoffs preserve the conversation.
"""

# Only necessary for Python versions prior to 3.11
from __future__ import annotations

import asyncio
import os
import sys
from contextlib import AsyncExitStack
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
import textwrap
from typing import Any, Dict, List, Optional

from agent_framework import ChatAgent, MCPStdioTool
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()

HANDOFF_PREFIX = "HANDOFF:"


@dataclass
class ConversationTurn:
    speaker: str
    role: str
    content: str


@dataclass(frozen=True)
class AzureOpenAIConfig:
    endpoint: str
    api_key: str
    deployment_name: str
    api_version: str = "2024-08-01-preview"

    @classmethod
    def from_env(cls) -> "AzureOpenAIConfig":
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", cls.api_version)
        if not all([endpoint, api_key, deployment_name]):
            raise ValueError("Azure OpenAI configuration missing required values")
        return cls(endpoint=endpoint, api_key=api_key, deployment_name=deployment_name, api_version=api_version)

    def to_client_kwargs(self) -> Dict[str, str]:
        return {
            "endpoint": self.endpoint,
            "api_key": self.api_key,
            "deployment_name": self.deployment_name,
            "api_version": self.api_version,
        }


@dataclass(frozen=True)
class AgentSpec:
    key: str
    name: str
    instructions: str
    tools: List[Any] = field(default_factory=list)
    display_name: Optional[str] = None


def build_agent_specs(current_year: int, current_date: str, travel_mcp_tool: MCPStdioTool) -> List[AgentSpec]:
    """Return the ordered list of agents participating in the workflow."""

    return [
        AgentSpec(
            key="coordinator",
            name="CoordinatorAgent",
            instructions=textwrap.dedent(
                """
                You are the CoordinatorAgent. Lead the conversation, capture requirements, and
                handover to specialists when needed. Announce handoffs with a line containing
                `HANDOFF:<Agent>` (for example `HANDOFF:Flight`). When you want control again,
                use `HANDOFF:Coordinator`.
                """
            ).strip(),
        ),
        AgentSpec(
            key="reference",
            name="ReferenceAgent",
            instructions=textwrap.dedent(
                f"""
                Provide airport, city, and time-zone reference data. Today is {current_date}.
                Always emit dates in YYYY-MM-DD with a four-digit year. After assisting, add
                `HANDOFF:Coordinator` on its own line.
                """
            ).strip(),
        ),
        AgentSpec(
            key="flight",
            name="FlightAgent",
            tools=[travel_mcp_tool],
            instructions=textwrap.dedent(
                f"""
                You are a flight specialist with access to a single `[FLIGHT] search_flight_offers`
                tool backed by Amadeus. It expects:
                  • `origin` / `destination` IATA codes
                  • `departure_date` (and optional `return_date`) in YYYY-MM-DD using the current year {current_year}
                  • `adults` passenger count and optional `travel_class`
                The tool returns plain text lines like `SEA → LHR | 820 USD | Cabin: ECONOMY`.
                Interpret relative dates before calling the tool, and keep results concise. Close
                with `HANDOFF:Coordinator` once you share guidance.
                """
            ).strip(),
        ),
        AgentSpec(
            key="hotel",
            name="HotelAgent",
            tools=[travel_mcp_tool],
            instructions=textwrap.dedent(
                f"""
                You can call the `[HOTEL] search_hotel_offers` tool to fetch properties. Provide:
                  • `city_code` (IATA-style city) and `check_in_date` / `check_out_date`
                  • `adults` count (default 1). Fill missing years with {current_year}.
                The tool responds with simple lines such as `Contoso Hotel | 210 USD | Check-in 2024-09-14`.
                Summarise the best matches and finish with `HANDOFF:Coordinator`.
                """
            ).strip(),
        ),
        AgentSpec(
            key="activity",
            name="ActivityAgent",
            instructions=textwrap.dedent(
                """
                Recommend activities, tours, and dining using open web knowledge and reasoning.
                No MCP tools are available for activities, so reference public information and
                past conversation context. Conclude with `HANDOFF:Coordinator` so the
                coordinator can continue planning.
                """
            ).strip(),
        ),
        AgentSpec(
            key="transfer",
            name="TransferAgent",
            instructions=textwrap.dedent(
                """
                Advise on airport transfers, ride services, and public transport using your
                general knowledge and the conversation history. MCP tools are not available for
                this domain, so lean on reasoning and practical tips. End with
                `HANDOFF:Coordinator` when finished.
                """
            ).strip(),
        ),
    ]

class TravelAgentOrchestrator:
    """Coordinates specialist agents using a handoff workflow."""

    def __init__(self) -> None:
        self.exit_stack = AsyncExitStack()
        self.agents: Dict[str, ChatAgent] = {}
        self.agent_threads: Dict[str, Any] = {}
        self.agent_labels: Dict[str, str] = {}
        self.current_agent_key = "coordinator"
        self.conversation: List[ConversationTurn] = []
        self.trip_notes: List[str] = []
        self.project_client: Optional[AIProjectClient] = None
        self.policy_agent_id: Optional[str] = None
        self.pending_auto_agent: Optional[str] = None
        self.pending_auto_prompt: Optional[str] = None
        self.last_user_message: str = ""

    async def initialize_agents(self) -> None:
        """Create Microsoft Agent Framework agents and the MCP bridge."""

        openai_config = AzureOpenAIConfig.from_env()
        chat_client = AzureOpenAIResponsesClient(**openai_config.to_client_kwargs())

        travel_server = Path(__file__).parent / "travel_mcp_server" / "server.py"
        if not travel_server.exists():
            raise FileNotFoundError(
                "travel_mcp_server/server.py was not found. Did you create the MCP server?"
            )

        travel_mcp_tool = MCPStdioTool(
            name="TravelMCP",
            command=sys.executable,
            args=[str(travel_server)],
        )

        today = datetime.now()
        current_date = today.strftime("%Y-%m-%d")
        current_year = today.year
        specs = build_agent_specs(current_year, current_date, travel_mcp_tool)
        for spec in specs:
            agent = await self._create_agent(chat_client, spec)
            self.agents[spec.key] = agent
            self.agent_threads[spec.key] = agent.get_new_thread()
            label = spec.display_name or getattr(agent, "display_name", None) or spec.key.title()
            self.agent_labels[spec.key] = label

        self.current_agent_key = "coordinator"

        foundry_endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
        agent_id = os.getenv("AZURE_AI_AGENT_ID")

        if foundry_endpoint and agent_id:
            self.project_client = AIProjectClient(
                endpoint=foundry_endpoint,
                credential=DefaultAzureCredential(),
            )
            self.policy_agent_id = agent_id
            print("[OK] Travel Policy Agent (Azure AI Foundry) configured")
        else:
            print("[INFO] Travel Policy Agent not configured (optional)")

        print("[OK] All agents initialised\n")

    async def _create_agent(self, chat_client: AzureOpenAIResponsesClient, spec: AgentSpec) -> ChatAgent:
        """Instantiate and register a ChatAgent under the shared exit stack."""

        return await self.exit_stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name=spec.name,
                instructions=spec.instructions,
                tools=spec.tools,
            )
        )

    async def check_travel_policy(self, trip_summary: str) -> Optional[str]:
        """Validate trip details against a persistent policy agent."""

        if not self.project_client or not self.policy_agent_id:
            return None

        try:
            agents_client = self.project_client.agents
            thread = agents_client.threads.create()
            agents_client.messages.create(
                thread_id=thread.id,
                role="user",
                content=[
                    {
                        "type": "text",
                        "text": f"Check if this itinerary complies with policy: {trip_summary}",
                    }
                ],
            )

            run = agents_client.runs.create(
                thread_id=thread.id,
                agent_id=self.policy_agent_id,
            )

            while run.status in ("queued", "in_progress"):
                await asyncio.sleep(0.5)
                run = agents_client.runs.get(thread_id=thread.id, run_id=run.id)

            if run.status == "completed":
                messages = agents_client.messages.list(thread_id=thread.id, order="desc")
                if messages:
                    latest = messages[0]
                    for item in latest.content:
                        if getattr(item, "text", None):
                            return item.text.value
            return "Policy check completed (no additional details returned)."
        except Exception as exc:
            return f"Policy check unavailable: {exc}"

    def render_context(self, limit: int = 6) -> str:
        """Return the last few turns as a compact summary."""

        recent = self.conversation[-limit:]
        lines = [f"{turn.speaker}: {turn.content}" for turn in recent]
        return "\n".join(lines)

    def build_agent_prompt(self, agent_key: str, user_input: str) -> str:
        """Combine prior conversation with the latest user request."""

        context = self.render_context()
        prompt_parts = []
        if context:
            prompt_parts.append("Conversation so far:\n" + context)
        prompt_parts.append(
            "Respond to the latest user request while staying consistent with the"
            " conversation. If you are handing control to another agent, output"
            " a line that starts with `HANDOFF:` before your final message."
        )
        prompt_parts.append("Latest user message:\n" + user_input)
        return "\n\n".join(prompt_parts)

    def record_turn(self, speaker: str, role: str, content: str) -> None:
        """Append a turn to the shared conversation log."""

        self.conversation.append(
            ConversationTurn(speaker=speaker, role=role, content=content.strip())
        )
        if len(self.conversation) > 40:
            self.conversation = self.conversation[-40:]

    def capture_trip_note(self, agent_key: str, text: str) -> None:
        """Store a concise note for the summary command."""

        if agent_key == "coordinator":
            return
        snippet = " ".join(text.strip().split())[:180]
        if snippet:
            self.trip_notes.append(f"{agent_key.title()}: {snippet}")
            if len(self.trip_notes) > 12:
                self.trip_notes = self.trip_notes[-12:]

    def parse_handoff(self, response_text: str) -> Optional[str]:
        """Detect explicit HANDOFF markers in agent responses."""

        for line in response_text.splitlines():
            line = line.strip()
            if line.upper().startswith(HANDOFF_PREFIX):
                target = line.split(":", maxsplit=1)[-1].strip().lower()
                mapping = {
                    "coordinator": "coordinator",
                    "flight": "flight",
                    "hotel": "hotel",
                    "activity": "activity",
                    "transfer": "transfer",
                    "reference": "reference",
                }
                return mapping.get(target)
        return None

    async def run_handoff_workflow(self) -> None:
        """Interactive console session using the handoff workflow."""

        print("=" * 70)
        print("Multi-Agent Travel Planning Assistant (Python)")
        print("Handoff workflow with Amadeus data via MCP")
        print("=" * 70)
        print("Type 'summary' for collected notes, 'policy' to validate, 'exit' to quit.\n")

        while True:
            try:
                auto_mode = False
                if (
                    self.pending_auto_agent
                    and self.pending_auto_agent in self.agents
                ):
                    self.current_agent_key = self.pending_auto_agent
                    auto_input = self.pending_auto_prompt or self.last_user_message or "Continue assisting based on the latest user request."
                    self.pending_auto_agent = None
                    self.pending_auto_prompt = None
                    user_input = auto_input
                    agent_label = self.agent_labels.get(
                        self.current_agent_key, self.current_agent_key.title()
                    )
                    auto_mode = True
                    print(f"\n[Auto] {agent_label} is responding...\n")
                else:
                    agent_label = self.agent_labels.get(
                        self.current_agent_key, self.current_agent_key.title()
                    )
                    user_input = input(f"You [{agent_label}]: ").strip()

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
                        print("\n[Policy Check] Running persistent agent validation...")
                        summary = " | ".join(self.trip_notes)
                        result = await self.check_travel_policy(summary)
                        print(f"{result or 'Policy check unavailable.'}\n")
                        continue

                    self.last_user_message = user_input
                    self.record_turn("User", "user", user_input)

                active_agent = self.agents[self.current_agent_key]
                prompt = self.build_agent_prompt(self.current_agent_key, user_input)
                thread = self.agent_threads[self.current_agent_key]

                print(f"\n{agent_label}: ", end="", flush=True)
                response_text = ""
                async for update in active_agent.run_stream(prompt, thread=thread):
                    if update.text:
                        print(update.text, end="", flush=True)
                        response_text += update.text
                print("\n")

                self.record_turn(agent_label, "assistant", response_text)
                self.capture_trip_note(self.current_agent_key, response_text)

                handoff_target = self.parse_handoff(response_text)
                if handoff_target and handoff_target in self.agents:
                    if handoff_target != self.current_agent_key:
                        self.current_agent_key = handoff_target
                        next_label = self.agent_labels.get(handoff_target, handoff_target.title())
                        if handoff_target == "coordinator":
                            print(f"[Handoff → {next_label}]\n")
                        else:
                            self.pending_auto_agent = handoff_target
                            self.pending_auto_prompt = (
                                response_text
                                or self.last_user_message
                                or "Continue assisting based on the latest user request."
                            )
                            print(f"[Handoff → {next_label}]\n")

            except KeyboardInterrupt:
                print("\nSession interrupted. Goodbye!\n")
                break
            except Exception as exc:
                print(f"\n[ERROR] {exc}\n")

    async def cleanup(self) -> None:
        """Dispose of agent contexts."""

        await self.exit_stack.aclose()


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
        print("Initialising travel agents...\n")
        await orchestrator.initialize_agents()
        await orchestrator.run_handoff_workflow()
    except Exception as exc:
        print(f"\n[FATAL] {exc}")
        import traceback

        traceback.print_exc()
        sys.exit(1)
    finally:
        await orchestrator.cleanup()


if __name__ == "__main__":
    asyncio.run(main())
