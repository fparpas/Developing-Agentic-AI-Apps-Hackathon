"""
Agentic RAG with Azure AI Search
This starter application demonstrates how to build an agentic retrieval system
using Azure AI Search and Azure OpenAI.
"""

import json
import os
import sys
import requests
from typing import List, Dict
from dotenv import load_dotenv
from colorama import Fore, Style, init
from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import (
    SearchIndex,
    SearchField,
    SearchFieldDataType,
    SimpleField,
    VectorSearch,
    VectorSearchProfile,
    HnswAlgorithmConfiguration,
    AzureOpenAIVectorizer,
    AzureOpenAIVectorizerParameters,
    SemanticConfiguration,
    SemanticSearch,
    SemanticPrioritizedFields,
    SemanticField,
    SearchIndexKnowledgeSource,
    SearchIndexKnowledgeSourceParameters,
    SearchIndexFieldReference,
    KnowledgeBase,
    KnowledgeBaseAzureOpenAIModel,
    KnowledgeSourceReference,
)
from azure.search.documents.knowledgebases import KnowledgeBaseRetrievalClient
from azure.search.documents.knowledgebases.models import (
    KnowledgeBaseRetrievalRequest,
    KnowledgeBaseMessage,
    KnowledgeBaseMessageTextContent,
)

# Load environment variables from .env file
load_dotenv()

# Initialize colorama for cross-platform color support
init(autoreset=True)


class AgenticRAGApp:
    """Main application class for Agentic RAG."""

    def __init__(self):
        """Initialize the application with environment variables."""
        self.aoai_endpoint = os.environ["AZURE_OPENAI_ENDPOINT"]
        self.aoai_key = os.environ["AZURE_OPENAI_API_KEY"]
        self.aoai_gpt_model = os.environ["AZURE_OPENAI_MODEL"]
        self.aoai_gpt_deployment = os.environ["AZURE_OPENAI_DEPLOYMENT_NAME"]
        self.aoai_embedding_model = os.environ["AZURE_OPENAI_EMBEDDINGS_MODEL"]
        self.aoai_embedding_deployment = os.environ["AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT_NAME"]
        self.search_endpoint = os.environ["AZURE_AI_SEARCH_ENDPOINT"]
        self.search_key = os.environ["AZURE_AI_SEARCH_KEY"]
        self.index_name = os.environ["AZURE_AI_SEARCH_INDEX_NAME"]
        self.knowledge_source_name = os.environ["AZURE_AI_SEARCH_KNOWLEDGE_SOURCE_NAME"]
        self.knowledge_agent_name = os.environ["AZURE_AI_SEARCH_KNOWLEDGE_AGENT_NAME"]
        self.index_client = None

    def register_agentic_search(self, index_data_url: str) -> SearchIndexClient:
        """
        Register and configure agentic search components.

        Args:
            index_data_url: URL to the data source for indexing

        Returns:
            SearchIndexClient instance
        """
        credential = DefaultAzureCredential()

        # TODO: Add your code here to complete this challenge
        # - Create a SearchIndexClient
        # - Upload data to the index
        # - Create a knowledge source
        # - Create a knowledge agent

        print(f"{Fore.RED}Register and configure agentic search components in code before running.")
        sys.exit(255)

        return None  # Replace with actual SearchIndexClient instance

    def cleanup_resources(self, index_client: SearchIndexClient):
        """
        Clean up Azure resources.

        Args:
            index_client: SearchIndexClient instance to use for cleanup
        """
        # Clean up resources
        index_client.delete_knowledge_base(self.knowledge_agent_name)
        print(f"Knowledge agent '{self.knowledge_agent_name}' deleted successfully.")

        index_client.delete_knowledge_source(self.knowledge_source_name)
        print(f"Knowledge source '{self.knowledge_source_name}' deleted successfully.")

        index_client.delete_index(self.index_name)
        print(f"Index '{self.index_name}' deleted successfully.")

    def start_interactive_chat(self, instructions: str):
        """
        Start an interactive chat session with the agentic retrieval system.

        Args:
            instructions: System instructions for the agent
        """
        search_endpoint = self.search_endpoint
        knowledge_agent_name = self.knowledge_agent_name

        print("\nAgentic Search Chat")
        print("--------------------")
        print("Type 'exit' to quit.\n")

        messages: List[Dict[str, str]] = [
            {
                "role": "system",
                "content": instructions,
            }
        ]

        agent_client = KnowledgeBaseRetrievalClient(
            endpoint=search_endpoint,
            knowledge_base_name=knowledge_agent_name,
            credential=AzureKeyCredential(self.search_key),
        )

        while True:
            try:
                user_input = input(f"{Fore.CYAN}User: {Style.RESET_ALL}")

                if not user_input or user_input.lower() == "exit":
                    break

                messages.append({"role": "user", "content": user_input})

                agent_messages = [
                    KnowledgeBaseMessage(
                        content=[KnowledgeBaseMessageTextContent(text=msg["content"])],
                        role=msg["role"],
                    )
                    for msg in messages
                    if msg["role"] != "system"
                ]

                retrieval_result = agent_client.retrieve(
                    retrieval_request=KnowledgeBaseRetrievalRequest(
                        messages=agent_messages,
                        output_mode="answerSynthesis",
                        include_activity=True,
                    )
                )

                assistant_response = retrieval_result.response[0].content[0].text
                messages.append({"role": "assistant", "content": assistant_response})

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
        # Print the synthesized answer
        answer = retrieval_result.response[0].content[0].text
        print(f"{Fore.YELLOW}\nAssistant:{Style.RESET_ALL} {answer}")

        # Print activity log
        if retrieval_result.activity:
            print(f"\n{Fore.LIGHTBLACK_EX}Activity:")
            for activity in retrieval_result.activity:
                activity_type = type(activity).__name__
                print(f"  {activity_type}")
                print(f"  {json.dumps(activity.as_dict(), indent=4, default=str)}")
            print(Style.RESET_ALL, end="")

        # Print source references
        if retrieval_result.references:
            print(f"{Fore.GREEN}\nReferences:")
            for ref in retrieval_result.references:
                print(f"  {json.dumps(ref.as_dict(), indent=4, default=str)}")
            print(Style.RESET_ALL, end="")



def main():
    """Main entry point for the application."""
    agent_instructions = (
        "A Q&A agent that can answer questions about the Earth at night. "
        "If you don't have the answer, respond with \"I don't know\"."
    )
    index_data_url = (
        "https://raw.githubusercontent.com/Azure-Samples/azure-search-sample-data/"
        "refs/heads/main/nasa-e-book/earth-at-night-json/documents.json"
    )

    app = AgenticRAGApp()

    # Register Agentic Search: create index, upload data, create knowledge source & agent
    index_client = app.register_agentic_search(index_data_url)

    # Start an interactive chat session
    app.start_interactive_chat(agent_instructions)

    # Clean up resources
    # This will delete the index, knowledge source, and knowledge agent,
    # it will not delete the Azure OpenAI / AI Search resource.
    #
    # app.cleanup_resources(index_client)


if __name__ == "__main__":
    main()
