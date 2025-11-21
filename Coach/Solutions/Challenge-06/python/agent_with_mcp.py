"""
Agent with MCP Integration - Challenge 06 Solution

This module integrates the Microsoft Agent Framework with MCP servers,
allowing agents to use MCP tools alongside native capabilities.
"""

import asyncio
import os
import sys
from contextlib import AsyncExitStack

from agent_framework import ChatAgent, MCPStdioTool
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

load_dotenv()


class MCPIntegratedAgent:
    """Agent that integrates with MCP servers for tool access."""

    def __init__(self):
        """Initialize the MCP-integrated agent."""
        self.agent = None
        self.exit_stack = AsyncExitStack()

    async def initialize_agent_with_mcp(self, server_script_path: str):
        """Initialize agent with MCP tool integration."""
        # Initialize Azure OpenAI client with API key
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

        # Create MCP tool for the weather server
        mcp_tool = MCPStdioTool(
            name="WeatherMCP",
            command="python",
            args=[server_script_path]
        )

        # Create agent with MCP tools
        instructions = """You are a helpful weather assistant with access to weather tools.
Use the available tools to answer questions accurately and provide detailed information."""

        self.agent = await self.exit_stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name="WeatherAgent",
                instructions=instructions,
                tools=[mcp_tool]
            )
        )

        print("Agent initialized with MCP tool integration\n")

    async def run_interactive_session(self, server_script_path: str):
        """Run interactive session with MCP-integrated agent."""
        print("Initializing MCP-Integrated Agent...")
        await self.initialize_agent_with_mcp(server_script_path)

        print("=" * 60)
        print("MCP-Integrated Weather Agent")
        print("=" * 60)
        print("Ask weather questions. Type 'exit' to quit.\n")

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

                # Non-streaming version (commented out):
                # response = await self.agent.run(query)
                # print(response)
                # print()

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
    if len(sys.argv) < 2:
        print("Usage: python agent_with_mcp.py <path_to_mcp_server>")
        sys.exit(1)

    server_path = sys.argv[1]

    agent = MCPIntegratedAgent()
    try:
        await agent.run_interactive_session(server_path)
    except Exception as e:
        print(f"\nError: {str(e)}")
        sys.exit(1)
    finally:
        await agent.cleanup()


if __name__ == "__main__":
    asyncio.run(main())
