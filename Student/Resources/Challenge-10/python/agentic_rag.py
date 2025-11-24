"""
Agentic RAG with Azure AI Search
This starter application demonstrates how to build an agentic retrieval system
using Azure AI Search and Azure OpenAI.
"""

import json
import os
from typing import List, Dict
from colorama import Fore, Style, init
from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.agents import KnowledgeAgentRetrievalClient
from azure.search.documents.agents.models import (
    KnowledgeAgentRetrievalRequest,
    KnowledgeAgentMessage,
    KnowledgeAgentMessageTextContent
)

# Initialize colorama for cross-platform color support
init(autoreset=True)


class AgenticRAGApp:
    """Main application class for Agentic RAG."""

    def __init__(self, config_path: str = "config.json"):
        """Initialize the application with configuration."""
        self.config = self._load_configuration(config_path)
        self.index_client = None

    def _load_configuration(self, config_path: str) -> dict:
        """Load configuration from JSON file."""
        if not os.path.exists(config_path):
            raise FileNotFoundError(f"Configuration file not found: {config_path}")

        with open(config_path, 'r') as f:
            return json.load(f)

    async def register_agentic_search(self, index_data_url: str) -> SearchIndexClient:
        """
        Register and configure agentic search components.

        Args:
            index_data_url: URL to the data source for indexing

        Returns:
            SearchIndexClient instance
        """
        # Load configuration settings
        aoai_endpoint = self.config["azure_openai"]["endpoint"]
        aoai_key = self.config["azure_openai"]["api_key"]
        aoai_gpt_model = self.config["azure_openai"]["model"]
        aoai_gpt_deployment = self.config["azure_openai"]["deployment_name"]
        aoai_embedding_model = self.config["azure_openai"]["embeddings_model"]
        aoai_embedding_deployment = self.config["azure_openai"]["embeddings_deployment_name"]

        search_endpoint = self.config["azure_ai_search"]["endpoint"]
        search_key = self.config["azure_ai_search"]["search_key"]
        index_name = self.config["azure_ai_search"]["index_name"]
        knowledge_source_name = self.config["azure_ai_search"]["knowledge_source_name"]
        knowledge_agent_name = self.config["azure_ai_search"]["knowledge_agent_name"]

        # Create a credential using DefaultAzureCredential
        credential = DefaultAzureCredential()

        # Add your code here to complete this challenge
        # - Create a SearchIndexClient
        # - Upload data to the index
        # - Create a knowledge source
        # - Create a knowledge agent

        return None  # Replace with actual SearchIndexClient instance

    async def cleanup_resources(self, index_client: SearchIndexClient):
        """
        Clean up Azure resources.

        Args:
            index_client: SearchIndexClient instance to use for cleanup
        """
        index_name = self.config["azure_ai_search"]["index_name"]
        knowledge_source_name = self.config["azure_ai_search"]["knowledge_source_name"]
        knowledge_agent_name = self.config["azure_ai_search"]["knowledge_agent_name"]

        # Clean up resources
        await index_client.delete_knowledge_agent(knowledge_agent_name)
        print(f"Knowledge agent '{knowledge_agent_name}' deleted successfully.")

        await index_client.delete_knowledge_source(knowledge_source_name)
        print(f"Knowledge source '{knowledge_source_name}' deleted successfully.")

        await index_client.delete_index(index_name)
        print(f"Index '{index_name}' deleted successfully.")

    async def start_interactive_chat(self, instructions: str):
        """
        Start an interactive chat session with the agentic retrieval system.

        Args:
            instructions: System instructions for the agent
        """
        search_endpoint = self.config["azure_ai_search"]["endpoint"]
        knowledge_agent_name = self.config["azure_ai_search"]["knowledge_agent_name"]

        print("Agentic Search Chat")
        print("Type 'exit' to quit.\n")

        # Set system message instructions
        messages: List[Dict[str, str]] = [
            {
                "role": "system",
                "content": instructions
            }
        ]

        # Use agentic retrieval to fetch results
        agent_client = KnowledgeAgentRetrievalClient(
            endpoint=search_endpoint,
            agent_name=knowledge_agent_name,
            credential=DefaultAzureCredential()
        )

        # Start chat loop
        while True:
            try:
                # Get user input
                user_input = input(f"{Fore.CYAN}User: {Style.RESET_ALL}")

                if not user_input or user_input.lower() == "exit":
                    break

                # Add user message to the conversation
                messages.append({
                    "role": "user",
                    "content": user_input
                })

                # Convert messages to the required format (exclude system messages)
                agent_messages = [
                    KnowledgeAgentMessage(
                        content=[KnowledgeAgentMessageTextContent(text=msg["content"])],
                        role=msg["role"]
                    )
                    for msg in messages if msg["role"] != "system"
                ]

                # Call the agentic retrieval client
                retrieval_result = await agent_client.retrieve(
                    retrieval_request=KnowledgeAgentRetrievalRequest(
                        messages=agent_messages
                    )
                )

                # Add assistant response to the conversation
                assistant_response = retrieval_result.response[0].content[0].text
                messages.append({
                    "role": "assistant",
                    "content": assistant_response
                })

                # Review the response, activity, and results
                self._review_response_activity_and_results(retrieval_result)
                print()

            except Exception as ex:
                print(f"{Fore.RED}Error: {ex}")
                print()

        print("Goodbye!")

    def _review_response_activity_and_results(self, retrieval_result):
        """
        Review and print the response, activity, and results.

        Args:
            retrieval_result: The retrieval result from the agent
        """
        # Print the response
        print(f"{Fore.YELLOW}Response:")
        print(retrieval_result.response[0].content[0].text)

        # Print the activity
        print(f"{Fore.LIGHTBLACK_EX}Activity:")
        for activity in retrieval_result.activity:
            print(f"Activity Type: {type(activity).__name__}")
            activity_json = json.dumps(
                activity.as_dict(),
                indent=2,
                default=str
            )
            print(activity_json)

        # Print the results
        print(f"{Fore.GREEN}Results:")
        for reference in retrieval_result.references:
            print(f"Reference Type: {type(reference).__name__}")
            reference_json = json.dumps(
                reference.as_dict(),
                indent=2,
                default=str
            )
            print(reference_json)



async def main():
    """Main entry point for the application."""
    agent_instructions = (
        "A Q&A agent that can answer questions about the Earth at night. "
        "If you don't have the answer, respond with \"I don't know\"."
    )
    index_data_url = (
        "https://raw.githubusercontent.com/Azure-Samples/azure-search-sample-data/"
        "refs/heads/main/nasa-e-book/earth-at-night-json/documents.json"
    )

    # Initialize the application
    app = AgenticRAGApp()

    # Register Agentic Search Tool
    # Implement code in the method above
    index_client = await app.register_agentic_search(index_data_url)

    # Start an interactive chat session
    await app.start_interactive_chat(agent_instructions)

    # Clean up resources
    await app.cleanup_resources(index_client)


if __name__ == "__main__":
    import asyncio
    asyncio.run(main())
