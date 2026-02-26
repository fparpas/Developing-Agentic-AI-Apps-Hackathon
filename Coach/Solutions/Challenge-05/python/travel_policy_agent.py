"""Travel Policy Compliance Agent - Challenge 05 Solution

Uses azure-ai-projects >= 1.0.0 (GA) with azure-ai-agents >= 1.1.0 (GA).
The AIProjectClient.agents property returns an authenticated AgentsClient
from the azure-ai-agents package.
"""

import os
import sys
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import AgentEventHandler, MessageDeltaChunk
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


class StreamingEventHandler(AgentEventHandler):
    """Custom event handler to stream agent responses in real-time."""

    def on_message_delta(self, delta: MessageDeltaChunk) -> None:
        """Handle streaming text deltas from the agent."""
        if delta.text:
            print(delta.text, end="", flush=True)


def process_query(user_message, agents_client, agent, thread):
    """Process a single user query with streaming."""

    agents_client.messages.create(
        thread_id=thread.id,
        role="user",
        content=[{"type": "text", "text": user_message}],
    )

    # Use streaming with custom event handler for real-time responses
    print("Assistant: ", end="", flush=True)

    with agents_client.runs.stream(
        thread_id=thread.id,
        agent_id=agent.id,
        event_handler=StreamingEventHandler(),
    ) as stream:
        stream.until_done()

    print()  # Extra newline for readability


def run_interactive_session(agents_client, agent, thread):
    """Run the interactive Q&A session."""

    print("\nAgent Service Ready! Enter your search queries or 'exit' to quit.")
    print("Commands:")
    print("   - Enter your query to search for information in the files")
    print("   - 'exit' - Quit application\n")

    while True:
        user_input = input("User Search> ").strip()

        if user_input.lower() == "exit":
            break

        if not user_input:
            continue

        try:
            process_query(user_input, agents_client, agent, thread)
        except Exception as ex:
            print(f"Sorry, I encountered an error: {ex}")

        print()


def main():
    """Main entry point."""

    print("Talk to Azure AI Agent Service")
    print("==============================")

    endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
    agent_id = os.getenv("AZURE_AI_AGENT_ID")

    if not endpoint or not agent_id:
        print(
            "Error: Set AZURE_AI_FOUNDRY_PROJECT_ENDPOINT and "
            "AZURE_AI_AGENT_ID environment variables"
        )
        sys.exit(1)

    # Use context manager for proper resource cleanup
    project_client = AIProjectClient(
        endpoint=endpoint,
        credential=DefaultAzureCredential(),
    )

    with project_client:
        # .agents returns an authenticated AgentsClient from azure-ai-agents
        agents_client = project_client.agents

        agent = agents_client.get_agent(agent_id)
        print(f"Loaded agent: {agent.name}")

        thread = agents_client.threads.create()
        print(f"Created thread, ID: {thread.id}")

        run_interactive_session(agents_client, agent, thread)


if __name__ == "__main__":
    main()
