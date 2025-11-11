"""
MCP Weather Client Agent - Challenge 03 Solution

This module demonstrates integrating MCP tools with the Agent Framework
using MCPStdioTool for easy tool discovery and execution from MCP servers.
"""

import asyncio
import os
import sys
from agent_framework import ChatAgent
from agent_framework.azure import AzureOpenAIResponsesClient
from agent_framework import MCPStdioTool
from dotenv import load_dotenv

# Load environment variables
load_dotenv()


async def main():
    """Main entry point for the MCP weather client agent."""
    if len(sys.argv) < 2:
        print("Usage: python mcp_weather_client.py <path_to_weather_server.py>")
        print("\nExample:")
        print("  python mcp_weather_client.py ../weather_mcp_server/weather.py")
        sys.exit(1)

    server_path = sys.argv[1]

    # Verify the server file exists
    if not os.path.exists(server_path):
        print(f"Error: Server file not found: {server_path}")
        sys.exit(1)

    # Get Azure OpenAI configuration
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    api_key = os.getenv("AZURE_OPENAI_API_KEY")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    api_version = os.getenv("AZURE_OPENAI_API_VERSION")

    # Validate configuration
    if not all([endpoint, api_key, deployment_name, api_version]):
        missing = [
            var for var, val in [
                ("AZURE_OPENAI_ENDPOINT", endpoint),
                ("AZURE_OPENAI_API_KEY", api_key),
                ("AZURE_OPENAI_DEPLOYMENT_NAME", deployment_name),
                ("AZURE_OPENAI_API_VERSION", api_version),
            ] if not val
        ]
        print("Error: Missing required environment variables:")
        for var in missing:
            print(f"  - {var}")
        sys.exit(1)

    # Create Azure OpenAI chat client
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
        args=[server_path]
    )

    # Create agent with MCP tools
    async with ChatAgent(
        chat_client=chat_client,
        name="WeatherAgent",
        instructions="You are a helpful weather assistant. Use the available MCP tools to answer weather questions accurately.",
        tools=[mcp_tool]
    ) as agent:
        print("\n" + "=" * 60)
        print("Weather Assistant")
        print("=" * 60)
        print("Ask me about the weather. Type 'exit' to quit.\n")

        # Interactive chat loop
        while True:
            try:
                user_input = input("You: ").strip()

                if user_input.lower() in ("exit", "quit", "bye"):
                    print("Goodbye!")
                    break

                if not user_input:
                    continue

                print("Agent: ", end="", flush=True)
                response = await agent.run(user_input)
                print(response)
                print()

            except KeyboardInterrupt:
                print("\n\nSession ended.")
                break
            except Exception as e:
                print(f"Error: {str(e)}\n")


if __name__ == "__main__":
    asyncio.run(main())
