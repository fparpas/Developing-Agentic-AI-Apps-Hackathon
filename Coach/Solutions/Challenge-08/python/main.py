"""
Challenge 08 Solution - Host the weather + remote MCP agent as a Foundry Hosted Agent (Python).

Takes the weather agent that integrates the remote Weather MCP server (Challenge 6, Task 2) and
hosts it on Microsoft Foundry Agent Service using the OpenAI-compatible Responses protocol.

FOUNDRY_PROJECT_ENDPOINT and AZURE_AI_MODEL_DEPLOYMENT_NAME are injected automatically when the
agent runs inside Foundry. Set them locally (plus WEATHER_MCP_ENDPOINT) for testing.
"""

import os

from agent_framework import Agent, MCPStreamableHTTPTool
from agent_framework.foundry import FoundryChatClient
from agent_framework_foundry_hosting import ResponsesHostServer
from azure.identity import DefaultAzureCredential
from dotenv import load_dotenv

load_dotenv()


def build_agent() -> Agent:
    """Build the weather agent backed by a Foundry model, with the remote MCP tools registered."""
    # 1. Chat client backed by the Foundry model deployment.
    client = FoundryChatClient(
        project_endpoint=os.environ["FOUNDRY_PROJECT_ENDPOINT"],
        model=os.environ["AZURE_AI_MODEL_DEPLOYMENT_NAME"],
        credential=DefaultAzureCredential(),
    )

    # 2. Connect to the remote Weather MCP server from Challenge 6.
    weather_mcp = MCPStreamableHTTPTool(
        name="WeatherMCP",
        url=os.environ["WEATHER_MCP_ENDPOINT"],
    )

    # 3. Build the agent and register the MCP tools.
    return Agent(
        client=client,
        name="weather-hosted-agent",
        instructions=(
            "You are a helpful assistant that answers weather questions "
            "using the available MCP tools."
        ),
        tools=[weather_mcp],
        # The hosting platform manages conversation history, so don't duplicate it.
        default_options={"store": False},
    )


def main() -> None:
    # 4. Host the agent behind the Foundry Responses protocol (/responses on http://localhost:8088).
    server = ResponsesHostServer(build_agent())
    server.run()


if __name__ == "__main__":
    main()
