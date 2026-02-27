"""Travel Policy Compliance Agent - Challenge 05 Solution"""

import os
import sys
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


def run_agent_conversation(project_endpoint, agent_id):
    """Run interactive conversation with the travel policy agent."""

    """
    TODO:
    Initialize the AI Project Client (use context manager: with ... as client)
    and start the conversation.

    Hints:
    - project_client = AIProjectClient(endpoint=..., credential=DefaultAzureCredential())
    - Use `with project_client:` for proper resource cleanup
    - agents_client = project_client.agents  # returns an AgentsClient
    - agent = agents_client.get_agent(agent_id)
    - thread = agents_client.threads.create()
    """


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

    run_agent_conversation(endpoint, agent_id)


if __name__ == "__main__":
    main()
