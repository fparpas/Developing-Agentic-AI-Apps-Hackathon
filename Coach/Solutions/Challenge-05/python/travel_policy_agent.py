"""Travel Policy Compliance Agent - Challenge 05 Solution"""

import asyncio
import os
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


async def run_agent_conversation(project_endpoint, agent_id):
    """Run interactive conversation with the travel policy agent."""

    endpoint = project_endpoint
    project_client = AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())

    agents_client = project_client.agents

    agent = agents_client.get_agent(agent_id)

    thread = agents_client.threads.create()
    print(f"Created thread, ID: {thread.id}")

    await run_interactive_session(agents_client, agent, thread)


async def run_interactive_session(agents_client, agent, thread):
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
            await process_query(user_input, agents_client, agent, thread)
        except Exception as ex:
            print(f"Sorry, I encountered an error: {ex}")

        print()


async def process_query(user_message, agents_client, agent, thread):
    """Process a single user query with streaming."""

    agents_client.messages.create(
        thread_id=thread.id,
        role="user",
        content=[{"type": "text", "text": user_message}]
    )

    # Use streaming to get real-time responses
    print("Assistant: ", end="", flush=True)

    with agents_client.runs.stream(
        thread_id=thread.id,
        agent_id=agent.id
    ) as stream:
        # The stream yields 3-tuples
        for event_type, event_data, _ in stream:
            if event_type == "thread.message.delta":
                # Extract text from delta content
                if hasattr(event_data, 'delta') and hasattr(event_data.delta, 'content'):
                    for content_part in event_data.delta.content:
                        if hasattr(content_part, 'text') and hasattr(content_part.text, 'value'):
                            print(content_part.text.value, end="", flush=True)

    print()  # Extra newline for readability


async def main():
    """Main entry point."""

    print("Talk to Azure AI Agent Service")
    print("==============================")

    endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
    agent_id = os.getenv("AZURE_AI_AGENT_ID")

    if not endpoint or not agent_id:
        raise ValueError("Missing required environment variables: AZURE_AI_FOUNDRY_PROJECT_ENDPOINT and AZURE_AI_AGENT_ID")

    await run_agent_conversation(endpoint, agent_id)


if __name__ == "__main__":
    asyncio.run(main())
