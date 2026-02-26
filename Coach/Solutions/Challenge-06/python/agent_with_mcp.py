"""
Agent with MCP Integration - Challenge 06 Solution

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

@tool(approval_mode="never_require")
def get_current_time_utc() -> str:
    """Returns the current system time in UTC."""
    return f"The current time in UTC is {datetime.now(timezone.utc).isoformat()}"


# ---------------------------------------------------------------------------
# Task 2: Agent Service integration
# ---------------------------------------------------------------------------

async def get_agent_from_service():
    """Retrieve an agent from Azure AI Foundry Agent Service."""
    endpoint = os.getenv("AZURE_AI_PROJECT_ENDPOINT")
    project_client = AIProjectClient(
        endpoint=endpoint,
        credential=DefaultAzureCredential(),
    )

    agents_client = project_client.agents
    agent_id = os.getenv("AGENT_ID")
    agent = agents_client.get_agent(agent_id)

    return agent, agents_client


# ---------------------------------------------------------------------------
# Task 3: MCP Weather server integration
# ---------------------------------------------------------------------------

async def integrate_mcp_tools_with_agent(agent: Agent, server_script_path: str):
    """Integrate MCP tools with the AI agent using MCPStdioTool."""
    mcp_tool = MCPStdioTool(
        name="WeatherMCP",
        command="python",
        args=[server_script_path],
    )

    if not hasattr(agent, "tools") or agent.tools is None:
        agent.tools = []

    agent.tools.append(mcp_tool)

    print(f"MCP tool integrated: {mcp_tool.name}")
    print("All MCP server tools are now available to the agent.")

    return agent


# ---------------------------------------------------------------------------
# Agent creation helpers
# ---------------------------------------------------------------------------

async def create_agent_with_tools() -> Agent:
    """Create an agent with the current time tool registered."""
    client = AzureOpenAIResponsesClient(
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        api_key=os.getenv("AZURE_OPENAI_API_KEY"),
        deployment_name=os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
        api_version=os.getenv("AZURE_OPENAI_API_VERSION", "latest"),
    )

    agent = Agent(
        client=client,
        name="TimeAgent",
        instructions=("""
            When the user asks for the current time, use the get_current_time_utc tool
            to provide an accurate response. Do not engage in any other type of conversation.
        """),
        tools=[get_current_time_utc],
    )

    return agent


# ---------------------------------------------------------------------------
# Interactive chat loop
# ---------------------------------------------------------------------------

async def main():
    """Main entry point for chatting with the agent."""
    agent = await create_agent_with_tools()

    # Task 3: Optionally integrate MCP weather tools
    server_path = sys.argv[1] if len(sys.argv) > 1 else None
    if server_path:
        agent = await integrate_mcp_tools_with_agent(agent, server_path)

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
