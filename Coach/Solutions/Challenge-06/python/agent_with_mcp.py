"""
Agent with MCP Integration - Challenge 06 Solution

This module integrates the Microsoft Agent Framework with MCP servers,
allowing agents to use MCP tools alongside native capabilities.
"""

import asyncio
import os
import sys
from contextlib import AsyncExitStack
from datetime import datetime

from agent_framework import ChatAgent, MCPStdioTool, AIFunction
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

load_dotenv()


class TimeTools:
    """Tools for providing time information."""

    @staticmethod
    def get_current_time_in_utc() -> str:
        """Returns the current system time in UTC."""
        return f"The current time in UTC is {datetime.utcnow()}"


class MCPIntegratedAgent:
    """Agent that integrates with MCP servers for tool access."""

    def __init__(self):
        """Initialize the MCP-integrated agent."""
        self.agent = None
        self.exit_stack = AsyncExitStack()

    async def create_simple_agent(self, agent_name: str, description: str, instructions: str):
        """Create a simple agent without any tools."""
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "latest")

        if not endpoint:
            raise ValueError("AZURE_OPENAI_ENDPOINT not set")
        if not api_key:
            raise ValueError("AZURE_OPENAI_API_KEY not set")
        if not deployment_name:
            raise ValueError("AZURE_OPENAI_DEPLOYMENT_NAME not set")

        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=api_key,
            deployment_name=deployment_name,
            api_version=api_version
        )

        self.agent = await self.exit_stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name=agent_name,
                instructions=instructions
            )
        )

        print(f"{agent_name} initialized\n")

    async def create_time_agent_and_register_tools(self, agent_name: str, instructions: str):
        """
        TASK 1: Create AI Time Agent and register Function tools.
        Complete this method to create the Time agent and register the TimeTools.get_current_time_in_utc function as a tool.
        """
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "latest")

        if not endpoint:
            raise ValueError("AZURE_OPENAI_ENDPOINT not set")
        if not api_key:
            raise ValueError("AZURE_OPENAI_API_KEY not set")
        if not deployment_name:
            raise ValueError("AZURE_OPENAI_DEPLOYMENT_NAME not set")

        # Create the chat client
        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=api_key,
            deployment_name=deployment_name,
            api_version=api_version
        )

        # Create an AIFunction from the TimeTools method
        time_function = AIFunction(
            name="get_current_time_in_utc",
            description="Returns the current system time in UTC",
            func=TimeTools.get_current_time_in_utc
        )

        # Create the agent with the time tool
        self.agent = await self.exit_stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name=agent_name,
                instructions=instructions,
                tools=[time_function]
            )
        )

        print(f"{agent_name} initialized with time tool\n")

    async def create_weather_agent_and_register_mcp_tools(self, agent_name: str, instructions: str, server_script_path: str):
        """
        TASK 2: Create AI Weather Agent and register MCP Weather tools.
        Complete this method to create the Weather agent and register the MCP Weather tools.
        """
        endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
        api_key = os.getenv("AZURE_OPENAI_API_KEY")
        deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
        api_version = os.getenv("AZURE_OPENAI_API_VERSION", "latest")

        if not endpoint:
            raise ValueError("AZURE_OPENAI_ENDPOINT not set")
        if not api_key:
            raise ValueError("AZURE_OPENAI_API_KEY not set")
        if not deployment_name:
            raise ValueError("AZURE_OPENAI_DEPLOYMENT_NAME not set")

        # Create the chat client
        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=api_key,
            deployment_name=deployment_name,
            api_version=api_version
        )

        # Create MCP tool for the weather server
        # MCPStdioTool automatically exposes all tools from the MCP server
        mcp_tool = MCPStdioTool(
            name="WeatherMCP",
            command="python",
            args=[server_script_path]
        )

        # Create the agent with the MCP tool
        self.agent = await self.exit_stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name=agent_name,
                instructions=instructions,
                tools=[mcp_tool]
            )
        )

        print(f"{agent_name} initialized with MCP weather tools\n")

    async def run_interactive_session(self, agent_name: str = "JokeAgent"):
        """Run interactive session with agent."""
        print("Initializing Agent...")

        # Create a simple joke-telling agent
        await self.create_simple_agent(
            agent_name="JokeAgent",
            description="A simple joke telling agent",
            instructions="You are a funny assistant that tells jokes."
        )

        # TASK 1: Create time agent
        await self.create_time_agent_and_register_tools(
            agent_name="TimeAgent",
            instructions="You are a helpful assistant that can provide time information using the available tools."
        )

        # TASK 2: Create weather agent with MCP tools
        server_path = sys.argv[1] if len(sys.argv) > 1 else None
        if server_path:
            await self.create_weather_agent_and_register_mcp_tools(
                agent_name="WeatherAgent",
                instructions="You are a helpful assistant that can provide weather information using the available tools.",
                server_script_path=server_path
            )

        print("=" * 60)
        print(f"Interactive Session for Agent: {self.agent.name if self.agent else 'Unknown'}")
        print("=" * 60)
        print("Type 'exit' to quit.\n")

        while True:
            try:
                query = input("You: ").strip()

                if query.lower() in ("exit", "quit"):
                    print("\nAgent: Goodbye!")
                    break

                if not query:
                    continue

                print("Agent: ", end="", flush=True)
                async for update in self.agent.run_stream(query):
                    if update.text:
                        print(update.text, end="", flush=True)
                print()

            except KeyboardInterrupt:
                print("\n\nAgent: Session ended.")
                break
            except Exception as e:
                print(f"\nError: {str(e)}\n")

    async def cleanup(self):
        """Clean up resources."""
        await self.exit_stack.aclose()


async def main():
    """Main entry point."""
    agent = MCPIntegratedAgent()
    try:
        await agent.run_interactive_session()
    except Exception as e:
        print(f"\nError: {str(e)}")
        sys.exit(1)
    finally:
        await agent.cleanup()


if __name__ == "__main__":
    asyncio.run(main())
