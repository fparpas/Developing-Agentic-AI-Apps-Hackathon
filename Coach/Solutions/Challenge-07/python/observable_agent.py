"""
Observability and Tracing for Agents - Challenge 07 Solution

This module demonstrates OpenTelemetry integration with Agent Framework
for comprehensive observability, tracing, and monitoring of agent operations.
"""

import asyncio
import os
import sys

from agent_framework import Agent, MCPStdioTool
from agent_framework.azure import AzureOpenAIResponsesClient
from agent_framework.observability import configure_otel_providers
from dotenv import load_dotenv
import logging

load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


def setup_observability():
    """Set up OpenTelemetry tracing via agent_framework's built-in support."""
    # agent_framework's configure_otel_providers sets up TracerProvider, MeterProvider,
    # and enables instrumentation for agent runs, chat completions, tool calls, and MCP.
    # Reads OTEL_EXPORTER_OTLP_TRACES_ENDPOINT from .env (traces only, no metrics to Jaeger).
    configure_otel_providers(
        enable_sensitive_data=True,
    )

    logger.info("OpenTelemetry observability configured")


async def run_session(server_script_path: str):
    """Run an observable agent session."""
    client = AzureOpenAIResponsesClient(
        endpoint=os.getenv("AZURE_OPENAI_ENDPOINT"),
        api_key=os.getenv("AZURE_OPENAI_API_KEY"),
        deployment_name=os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME"),
        api_version=os.getenv("AZURE_OPENAI_API_VERSION", "latest"),
    )

    mcp_tool = MCPStdioTool(
        name="WeatherMCP",
        command="python",
        args=[server_script_path],
    )

    async with Agent(
        client=client,
        name="ObservableWeatherAgent",
        instructions="You are a helpful weather assistant with full observability.",
        tools=[mcp_tool],
    ) as agent:
        print("=" * 60)
        print("Observable Weather Agent with Tracing")
        print("=" * 60)
        print("All interactions are being traced for observability.")
        print("Type 'exit' to quit.\n")

        query_count = 0
        while True:
            try:
                query = input("You: ").strip()
                if query.lower() in ("exit", "quit"):
                    break
                if not query:
                    continue

                query_count += 1
                try:
                    result = await agent.run(query)
                    print(f"Agent: {result}\n")
                except Exception as e:
                    logger.error(f"Error processing query: {e}")
                    print(f"\nError: {e}\n")

            except KeyboardInterrupt:
                break

        logger.info(f"Session ended. Processed {query_count} queries")


async def main():
    if len(sys.argv) < 2:
        print("Usage: python observable_agent.py <path_to_mcp_server>")
        sys.exit(1)

    setup_observability()
    await run_session(sys.argv[1])


if __name__ == "__main__":
    asyncio.run(main())
