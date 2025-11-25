"""Travel Policy Compliance Agent - Challenge 05 Solution"""

import asyncio
import os
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


async def run_agent_conversation(project_endpoint, agent_id):
    """Run interactive conversation with the travel policy agent."""

    """
    TODO:
    Initialize the AI Project Client and start the conversation.
    """


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
