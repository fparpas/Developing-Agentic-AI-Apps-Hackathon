# Agentic RAG with Azure AI Search - Python Starter

This is the starter project for Challenge 10 - Build Agentic RAG with Azure AI Search (Python version).

## Prerequisites

- Python 3.8 or higher
- Azure AI Search service (Standard tier or higher)
- Azure OpenAI service with deployed models:
  - Chat completion model (e.g., `gpt-4o`)
  - Embedding model (e.g., `text-embedding-3-small`)

## Setup

1. **Install dependencies**:
   ```bash
   pip install -r requirements.txt

   # or
   # uv pip install -r requirements.txt
   # if using uv (highly recommended for performance)
   # https://docs.astral.sh/uv/
   ```

2. **Configure the application**:
   - Copy `config.json` and update it with your Azure service credentials
   - Fill in the following values:
     - Azure OpenAI endpoint and API key
     - Azure OpenAI deployment names and models
     - Azure AI Search endpoint and search key
     - Index name, knowledge source name, and knowledge agent name

3. **Configure permissions** (required for agentic retrieval):
   - Enable RBAC on your Azure AI Search service
   - Enable system-assigned managed identity on the Search service
   - Assign these roles to yourself:
     - `Search Service Contributor`
     - `Search Index Data Contributor`
     - `Search Index Data Reader`
   - Assign `Cognitive Services OpenAI User` role to the Search service's managed identity

## Your Task

Complete the implementation in the `register_agentic_search()` method in `agentic_rag.py`:

1. Create a search index with:
   - Document fields (id, page_chunk, page_embedding_text_3_large, page_number)
   - Vector search configuration
   - Semantic search configuration

2. Upload NASA "Earth at Night" data to the index

3. Create a knowledge source that references the index

4. Create a knowledge agent that connects Azure OpenAI with the knowledge source

## Running the Application

```bash
python agentic_rag.py
```

The application will:
1. Set up the agentic search components (once you complete the implementation)
2. Start an interactive chat session
3. Clean up resources when you exit

## Project Structure

```
python/
├── agentic_rag.py        # Main application file
├── config.json           # Configuration file (you need to update this)
├── requirements.txt      # Python dependencies
└── README.md             # This file
```

## Tips

- Refer to the Challenge 10 README for detailed implementation guidance
- Use the Azure AI Search Python SDK documentation for API reference
- The conversational interface is already implemented - focus on the retrieval components
- Test your implementation with various question types to observe agentic behavior

## Resources

- [Azure AI Search Python SDK](https://learn.microsoft.com/python/api/overview/azure/search-documents-readme)
- [Azure Identity Python SDK](https://learn.microsoft.com/python/api/overview/azure/identity-readme)
- [Agentic Retrieval Concepts](https://learn.microsoft.com/azure/search/search-agentic-retrieval-concept)
