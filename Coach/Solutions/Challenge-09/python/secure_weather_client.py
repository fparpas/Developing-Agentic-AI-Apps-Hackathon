"""Secure MCP Client - connects to a remote MCP server with API key auth."""

import asyncio
import os
from agent_framework import ChatAgent, MCPStreamableHTTPTool
from agent_framework.azure import AzureOpenAIResponsesClient
from dotenv import load_dotenv

load_dotenv()


async def main():
    chat_client = AzureOpenAIResponsesClient(
        endpoint=os.environ["AZURE_OPENAI_ENDPOINT"],
        api_key=os.environ["AZURE_OPENAI_API_KEY"],
        deployment_name=os.environ["AZURE_OPENAI_DEPLOYMENT_NAME"],
        api_version="latest"
    )

    # The only change vs. an unsecured client: pass the API key via headers
    server_url = os.getenv("MCP_SERVER_URL", "http://localhost:8000/mcp")
    mcp_tool = MCPStreamableHTTPTool(
        name="WeatherMCP",
        url=server_url,
        headers={"X-API-Key": os.environ["API_KEY"]}
    )

    async with ChatAgent(
        chat_client=chat_client,
        name="WeatherAgent",
        instructions="You are a helpful weather assistant with access to weather tools.",
        tools=[mcp_tool]
    ) as agent:
        print("\nAsk weather questions (type 'exit' to quit):\n")
        while True:
            user_input = input("You: ").strip()
            if user_input.lower() in ("exit", "quit", "q"):
                break
            if not user_input:
                continue

            print("Agent: ", end="", flush=True)
            async for update in agent.run_stream(user_input):
                if update.text:
                    print(update.text, end="", flush=True)
            print("\n")


if __name__ == "__main__":
    asyncio.run(main())
