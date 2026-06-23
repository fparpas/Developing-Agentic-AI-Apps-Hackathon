"""
Agentic RAG with Azure AI Search - Solution
This application demonstrates how to build an agentic retrieval system
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

        # ── 1. Create the Search Index ──────────────────────────────────
        fields = [
            SimpleField(
                name="id",
                type=SearchFieldDataType.String,
                key=True,
                filterable=True,
                sortable=True,
                facetable=True,
            ),
            SearchField(
                name="page_chunk",
                type=SearchFieldDataType.String,
                filterable=False,
                sortable=False,
                facetable=False,
            ),
            SearchField(
                name="page_embedding_text_3_large",
                type=SearchFieldDataType.Collection(SearchFieldDataType.Single),
                vector_search_dimensions=3072,
                vector_search_profile_name="hnsw_text_3_large",
            ),
            SimpleField(
                name="page_number",
                type=SearchFieldDataType.Int32,
                filterable=True,
                sortable=True,
                facetable=True,
            ),
        ]

        vectorizer = AzureOpenAIVectorizer(
            vectorizer_name="azure_openai_text_3_large",
            parameters=AzureOpenAIVectorizerParameters(
                resource_url=self.aoai_endpoint,
                deployment_name=self.aoai_embedding_deployment,
                model_name=self.aoai_embedding_model,
            ),
        )

        vector_search = VectorSearch(
            profiles=[
                VectorSearchProfile(
                    name="hnsw_text_3_large",
                    algorithm_configuration_name="alg",
                    vectorizer_name="azure_openai_text_3_large",
                )
            ],
            algorithms=[HnswAlgorithmConfiguration(name="alg")],
            vectorizers=[vectorizer],
        )

        semantic_config = SemanticConfiguration(
            name="semantic_config",
            prioritized_fields=SemanticPrioritizedFields(
                content_fields=[SemanticField(field_name="page_chunk")]
            ),
        )

        semantic_search = SemanticSearch(
            default_configuration_name="semantic_config",
            configurations=[semantic_config],
        )

        index = SearchIndex(
            name=self.index_name,
            fields=fields,
            vector_search=vector_search,
            semantic_search=semantic_search,
        )

        index_client = SearchIndexClient(
            endpoint=self.search_endpoint,
            credential=AzureKeyCredential(self.search_key),
        )
        index_client.create_or_update_index(index)
        print(f"Index '{self.index_name}' created or updated successfully.")

        # ── 2. Upload Data to the Index ─────────────────────────────────
        response = requests.get(index_data_url, timeout=60)
        response.raise_for_status()
        documents = response.json()

        search_client = SearchClient(
            endpoint=self.search_endpoint,
            index_name=self.index_name,
            credential=AzureKeyCredential(self.search_key),
        )
        search_client.upload_documents(documents=documents)
        print(f"Documents uploaded to index '{self.index_name}' successfully.")

        # ── 3. Create a Knowledge Source ────────────────────────────────
        index_knowledge_source = SearchIndexKnowledgeSource(
            name=self.knowledge_source_name,
            search_index_parameters=SearchIndexKnowledgeSourceParameters(
                search_index_name=self.index_name,
                source_data_fields=[
                    SearchIndexFieldReference(name="id"),
                    SearchIndexFieldReference(name="page_chunk"),
                    SearchIndexFieldReference(name="page_number"),
                ],
            ),
        )
        index_client.create_or_update_knowledge_source(index_knowledge_source)
        print(f"Knowledge source '{self.knowledge_source_name}' created or updated successfully.")

        # ── 4. Create a Knowledge Agent (Knowledge Base) ────────────────
        openai_parameters = AzureOpenAIVectorizerParameters(
            resource_url=self.aoai_endpoint,
            deployment_name=self.aoai_gpt_deployment,
            model_name=self.aoai_gpt_model,
        )

        agent_model = KnowledgeBaseAzureOpenAIModel(
            azure_open_ai_parameters=openai_parameters
        )

        agent = KnowledgeBase(
            name=self.knowledge_agent_name,
            models=[agent_model],
            knowledge_sources=[
                KnowledgeSourceReference(
                    name=self.knowledge_source_name,
                )
            ],
        )

        index_client.create_or_update_knowledge_base(agent)
        print(f"Knowledge agent '{self.knowledge_agent_name}' created or updated successfully.")

        self.index_client = index_client
        return index_client

    def cleanup_resources(self, index_client: SearchIndexClient):
        """
        Clean up Azure resources.

        Args:
            index_client: SearchIndexClient instance to use for cleanup
        """
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
        'If you don\'t have the answer, respond with "I don\'t know".'
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
