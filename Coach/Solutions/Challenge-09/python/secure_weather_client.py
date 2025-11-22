"""
Secure MCP Weather Client - Challenge 09 Solution

This client demonstrates how to connect to a secure remote MCP server
that requires API key authentication. It showcases:
- API key header injection for authentication
- Remote MCP server communication via SSE transport
- Interactive chat interface using Microsoft Agent Framework
- Azure OpenAI integration for natural language queries
"""

import asyncio
import logging
import os
import sys
from contextlib import AsyncExitStack
from urllib.parse import urlparse

from agent_framework import ChatAgent, MCPStreamableHTTPTool
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

# ANSI color codes
ORANGE = "\033[38;5;208m"
RESET = "\033[0m"


def mcp_debug(message: str) -> None:
    """Print MCP debug messages to stderr in orange."""
    print(f"{ORANGE}[MCP] {message}{RESET}", file=sys.stderr, flush=True)


async def main():
    """
    Main entry point for the secure MCP client.
    Connects to a remote MCP server with API key authentication.
    """
    # Suppress asyncio error logs for cleaner output on exit
    logging.getLogger("asyncio").setLevel(logging.CRITICAL)

    print("=" * 60)
    print("Secure MCP Weather Client - Challenge 09 Solution")
    print("=" * 60)
    print()

    # ==========================
    # STEP 1: Load Configuration
    # ==========================
    # We support both local (stdio) and remote (SSE) MCP servers
    # Remote servers require API key authentication

    # MCP Server Configuration
    use_local = os.getenv("USE_LOCAL_MCP", "false").lower() == "true"

    if use_local:
        print("WARNING: Local MCP mode not implemented in this solution")
        print("This challenge focuses on remote, secured MCP servers")
        sys.exit(1)

    # Remote MCP server settings
    remote_server_url = os.getenv("MCP_SERVER_URL")
    api_key = os.getenv("API_KEY")

    if not remote_server_url:
        print("ERROR: MCP_SERVER_URL not set in .env file")
        print("Please configure the remote MCP server endpoint")
        sys.exit(1)

    if not api_key:
        print("ERROR: API_KEY not set in .env file")
        print("Please configure the API key for authentication")
        sys.exit(1)

    # Ensure the URL points to the MCP endpoint
    if not remote_server_url.endswith("/mcp"):
        remote_server_url = remote_server_url.rstrip("/") + "/mcp"

    # Extract host and port for debug message
    parsed_url = urlparse(remote_server_url)
    host = parsed_url.hostname or "localhost"
    port = parsed_url.port or (443 if parsed_url.scheme == "https" else 80)

    mcp_debug(f"Connecting to MCP Server {host} on port {port}")

    # Azure OpenAI Configuration
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    openai_key = os.getenv("AZURE_OPENAI_API_KEY")
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    api_version = os.getenv("AZURE_OPENAI_API_VERSION", "2024-10-21")

    if not all([endpoint, openai_key, deployment_name]):
        print("ERROR: Azure OpenAI configuration incomplete")
        print("Required environment variables:")
        print("  - AZURE_OPENAI_ENDPOINT")
        print("  - AZURE_OPENAI_API_KEY")
        print("  - AZURE_OPENAI_DEPLOYMENT_NAME")
        sys.exit(1)

    print(f"[OK] Configuration loaded")
    print(f"  - MCP Server: {remote_server_url}")
    print(f"  - Azure OpenAI: {deployment_name}")
    print()

    # =============================================
    # STEP 2: Initialize Agent with Secure MCP Tool
    # =============================================
    # Use AsyncExitStack for proper async resource cleanup
    async with AsyncExitStack() as stack:

        # Create Azure OpenAI chat client
        chat_client = AzureOpenAIResponsesClient(
            endpoint=endpoint,
            api_key=openai_key,
            deployment_name=deployment_name,
            api_version=api_version
        )

        print("[OK] Azure OpenAI client initialized")

        # Create MCP tool for remote HTTP server with API key authentication
        # The headers parameter injects X-API-Key on every request
        mcp_debug("Configuring MCP tool with API key authentication")
        mcp_tool = MCPStreamableHTTPTool(
            name="WeatherMCP",
            url=remote_server_url,
            headers={"X-API-Key": api_key}
        )

        print("[OK] Secure MCP tool configured with API key authentication")

        # Create the chat agent with MCP tools
        # The agent automatically decides when to call MCP tools
        mcp_debug("Initializing chat agent with MCP tools")
        agent = await stack.enter_async_context(
            ChatAgent(
                chat_client=chat_client,
                name="WeatherAgent",
                instructions="""You are a helpful weather assistant with access to weather tools.
When users ask about weather, use the available tools to get real-time data.
Provide clear, friendly responses with the weather information.""",
                tools=[mcp_tool]
            )
        )

        mcp_debug("Successfully connected to MCP server")
        print("[OK] Agent initialized with secure MCP access")
        print()

        # =============================
        # STEP 3: Interactive Chat Loop
        # =============================
        print("\nAsk weather questions like:")
        print("  - What's the weather in Seattle?")
        print("  - Are there any alerts for California?")
        print("  - Get forecast for latitude 40.7128, longitude -74.0060")
        print()
        print("Type 'exit' or 'quit' to end the session")
        print("=" * 60)
        print()

        while True:
            try:
                # Get user input
                user_query = input("You: ").strip()

                # Check for exit commands
                if user_query.lower() in ("exit", "quit", "q"):
                    mcp_debug("Disconnecting from MCP server")
                    print("\nGoodbye! Disconnecting from secure MCP server...")
                    break

                # Skip empty input
                if not user_query:
                    continue

                # Send query to agent and stream the response
                # The agent will automatically call MCP tools if needed
                print("Agent: ", end="", flush=True)

                async for update in agent.run_stream(user_query):
                    if update.text:
                        print(update.text, end="", flush=True)

                print("\n")  # Add newline after response

            except KeyboardInterrupt:
                print("\n\nSession interrupted. Goodbye!")
                break

            except Exception as e:
                print(f"\nERROR: {str(e)}\n")
                # Continue the loop to allow user to try again


if __name__ == "__main__":
    asyncio.run(main())
