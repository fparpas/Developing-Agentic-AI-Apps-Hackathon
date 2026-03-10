"""
Agent with MCP Integration - Challenge 06

This module demonstrates:
  Task 1 - Current time tool using the @tool decorator
  Task 2 - Agent Service integration for travel policy compliance
  Task 3 - MCP Weather server integration via MCPStdioTool
"""

import asyncio
import os
import sys
from datetime import datetime, timezone

from agent_framework import Agent, MCPStdioTool, tool
from agent_framework.azure import AzureOpenAIResponsesClient
from azure.ai.projects import AIProjectClient
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


# ---------------------------------------------------------------------------
# Task 1: Current time tool using @tool decorator
# ---------------------------------------------------------------------------
# TODO: Create a function decorated with @tool that returns the current UTC time.
# Hints:
#   - Use the @tool decorator with approval_mode="never_require"
#   - The function should return a string with the current UTC time
#   - Use datetime.now(timezone.utc).isoformat() for formatting


# ---------------------------------------------------------------------------
# Task 2: Agent Service integration
# ---------------------------------------------------------------------------
# TODO: Implement get_agent_from_service() to retrieve an agent from
# Azure AI Foundry Agent Service.
# Hints:
#   - Create an AIProjectClient using AZURE_AI_PROJECT_ENDPOINT and DefaultAzureCredential
#   - Use project_client.agents to get the agents client
#   - Retrieve the agent by its ID from the AGENT_ID environment variable
#   - Return both the agent and the agents_client


# ---------------------------------------------------------------------------
# Task 3: MCP Weather server integration
# ---------------------------------------------------------------------------
# TODO: Implement integrate_mcp_tools_with_agent() to add MCP weather tools
# to the agent.
# Hints:
#   - Create an MCPStdioTool with name="WeatherMCP", command="python",
#     and args=[server_script_path]
#   - Append the MCP tool to agent.tools
#   - Return the updated agent


# ---------------------------------------------------------------------------
# Agent creation
# ---------------------------------------------------------------------------

async def create_agent_with_tools() -> Agent:
    """Create an agent with the current time tool registered."""
    client = AzureOpenAIResponsesClient(
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        api_key=os.getenv("AZURE_OPENAI_API_KEY"),
        deployment_name=os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
        api_version=os.getenv("AZURE_OPENAI_API_VERSION", "latest"),
    )

    # TODO: Create an Agent with:
    #   - client=client
    #   - name="TimeAgent"
    #   - instructions that tell the agent to use get_current_time_utc for time queries
    #   - tools=[get_current_time_utc]  (pass your @tool-decorated function)
    # Then return the agent.
    raise NotImplementedError("Complete create_agent_with_tools()")


# ---------------------------------------------------------------------------
# Interactive chat loop
# ---------------------------------------------------------------------------

async def main():
    """Main entry point for chatting with the agent."""
    agent = await create_agent_with_tools()

    # Task 3: Optionally integrate MCP weather tools
    server_path = sys.argv[1] if len(sys.argv) > 1 else None
    if server_path:
        # TODO: Uncomment the line below once you implement integrate_mcp_tools_with_agent()
        # agent = await integrate_mcp_tools_with_agent(agent, server_path)
        pass

    print("Chat with the agent (type 'exit' to quit)")
    print("-" * 50)

    while True:
        try:
            user_input = input("\nYou: ").strip()

            if user_input.lower() in ("exit", "quit"):
                print("Bu-bye!")
                break

            if not user_input:
                continue

            # Stream response from agent token-by-token
            print("\nAgent: ", end="", flush=True)
            async for update in agent.run(user_input, stream=True):
                if update.text:
                    print(update.text, end="", flush=True)
            print()

        except KeyboardInterrupt:
            print("\n\nSession ended.")
            break


if __name__ == "__main__":
    asyncio.run(main())
