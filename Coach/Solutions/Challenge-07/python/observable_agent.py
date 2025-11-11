"""
Observability and Tracing for Agents - Challenge 07 Solution

This module demonstrates OpenTelemetry integration with Agent Framework
for comprehensive observability, tracing, and monitoring of agent operations.
"""

import asyncio
import os
import sys
from contextlib import AsyncExitStack

from opentelemetry import trace, metrics
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.instrumentation.httpx import HTTPXClientInstrumentor
from opentelemetry.instrumentation.requests import RequestsInstrumentor

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from agent_framework import ChatAgent
from agent_framework.openai import OpenAIResponsesClient
from dotenv import load_dotenv
import logging

load_dotenv()

# Configure logging for observability
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class ObservableAgent:
    """Agent with built-in observability and tracing."""

    def __init__(self):
        """Initialize the observable agent with tracing."""
        self.agent = None
        self.session = None
        self.exit_stack = AsyncExitStack()
        self.mcp_tools = []
        self.tracer = None
        self.meter = None

    def setup_observability(self):
        """Set up OpenTelemetry for tracing and metrics."""
        # Get OTLP endpoint (e.g., from environment or Jaeger)
        otlp_endpoint = os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "localhost:4317")

        # Setup trace provider
        trace_exporter = OTLPSpanExporter(endpoint=otlp_endpoint)
        trace_provider = TracerProvider()
        trace_provider.add_span_processor(BatchSpanProcessor(trace_exporter))
        trace.set_tracer_provider(trace_provider)

        # Setup metrics provider
        metric_reader = PeriodicExportingMetricReader(
            OTLPMetricExporter(endpoint=otlp_endpoint)
        )
        metrics_provider = MeterProvider(metric_readers=[metric_reader])
        metrics.set_meter_provider(metrics_provider)

        # Get tracer and meter
        self.tracer = trace.get_tracer(__name__)
        self.meter = metrics.get_meter(__name__)

        # Instrument common libraries
        HTTPXClientInstrumentor().instrument()
        RequestsInstrumentor().instrument()

        logger.info("OpenTelemetry observability configured")

    async def connect_to_mcp_server(self, server_script_path: str):
        """Connect to an MCP server with tracing."""
        with self.tracer.start_as_current_span("connect_to_mcp_server") as span:
            span.set_attribute("server_path", server_script_path)

            server_params = StdioServerParameters(
                command="python",
                args=[server_script_path]
            )

            logger.info(f"Connecting to MCP server: {server_script_path}")

            stdio_transport = await self.exit_stack.enter_async_context(
                stdio_client(server_params)
            )
            self.stdio, self.write = stdio_transport

            self.session = await self.exit_stack.enter_async_context(
                ClientSession(self.stdio, self.write)
            )
            await self.session.initialize()

            response = await self.session.list_tools()
            self.mcp_tools = response.tools

            tool_names = [tool.name for tool in self.mcp_tools]
            span.set_attribute("tools", tool_names)
            logger.info(f"Connected with tools: {tool_names}")

    async def initialize_agent_with_tracing(self):
        """Initialize agent with observability."""
        with self.tracer.start_as_current_span("initialize_agent") as span:
            api_key = os.getenv("OPENAI_API_KEY")
            if not api_key:
                raise ValueError("OPENAI_API_KEY not set")

            chat_client = OpenAIResponsesClient(
                api_key=api_key,
                model="gpt-4o-mini"
            )

            self.agent = await self.exit_stack.enter_async_context(
                ChatAgent(
                    chat_client=chat_client,
                    name="ObservableWeatherAgent",
                    instructions="You are a helpful weather assistant with full observability."
                )
            )

            span.set_attribute("agent_name", "ObservableWeatherAgent")
            logger.info("Agent initialized with observability")

    async def process_query_with_tracing(self, query: str) -> str:
        """Process a query with comprehensive tracing."""
        with self.tracer.start_as_current_span("process_query") as span:
            # Add query to trace
            span.set_attribute("query", query[:100])  # Log first 100 chars

            # Record query processing
            logger.info(f"Processing query: {query}")

            try:
                result = await self.agent.run(query)

                # Add result to trace
                span.set_attribute("result_length", len(result))
                span.set_attribute("status", "success")

                logger.info(f"Query processed successfully")

                return result

            except Exception as e:
                # Record error in trace
                span.set_attribute("error", str(e))
                span.set_attribute("status", "error")

                logger.error(f"Error processing query: {str(e)}")
                return f"Error: {str(e)}"

    async def run_observable_session(self, server_script_path: str):
        """Run session with full observability."""
        with self.tracer.start_as_current_span("session_start"):
            logger.info("Starting observable agent session")

            await self.connect_to_mcp_server(server_script_path)
            await self.initialize_agent_with_tracing()

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

                    print("Agent: ", end="", flush=True)
                    response = await self.process_query_with_tracing(query)
                    print(response)
                    print()

                except KeyboardInterrupt:
                    logger.info("Session interrupted by user")
                    break
                except Exception as e:
                    logger.error(f"Session error: {str(e)}")
                    print(f"\nError: {str(e)}\n")

            logger.info(f"Session ended. Processed {query_count} queries")

    async def cleanup(self):
        """Clean up resources."""
        await self.exit_stack.aclose()
        logger.info("Resources cleaned up")


async def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python observable_agent.py <path_to_mcp_server>")
        sys.exit(1)

    server_path = sys.argv[1]

    agent = ObservableAgent()
    agent.setup_observability()

    try:
        await agent.run_observable_session(server_path)
    except Exception as e:
        logger.error(f"Fatal error: {str(e)}")
        sys.exit(1)
    finally:
        await agent.cleanup()


if __name__ == "__main__":
    asyncio.run(main())
