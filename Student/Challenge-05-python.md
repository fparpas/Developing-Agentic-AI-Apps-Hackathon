# Challenge 05 - Python - Build your first AI Agent with AI Agents Service

 [< Previous Challenge](./Challenge-04-python.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-06-python.md)

[![](https://img.shields.io/badge/C%20Sharp-lightgray)](Challenge-05-csharp.md)
[![](https://img.shields.io/badge/Python-blue)](Challenge-05-python.md)

## Introduction

In this challenge, you'll build your first AI Agent using Azure AI Agents Service with file search capabilities. You'll create a **Travel Policy Compliance Agent** that can analyze and answer questions about your company's travel policy using intelligent document retrieval.

## Concepts

Before diving into the implementation, let's understand the key concepts of Azure AI Agent Service

### Azure AI Agents Service

Azure AI Agents Service provides a managed platform for building and deploying AI agents with advanced capabilities. Key features include:

- **Managed Agent Hosting**: Azure handles the infrastructure, scaling, and management of your AI agents
- **Built-in Tools**: Pre-configured tools for file search, function calling, and code interpretation
- **Persistent Conversations**: Maintain context across multiple interactions with thread management
- **Secure Authentication**: Integrated Azure Identity for secure access control

### File Search and Vector Stores

File search capability enables your agent to retrieve relevant information from uploaded documents:

- **Vector Stores**: Azure-managed storage for documents that are automatically chunked, embedded, and indexed
- **Semantic Search**: Find relevant content based on meaning, not just keyword matching
- **Retrieval-Augmented Generation (RAG)**: Combine document retrieval with AI generation for accurate, contextual responses
- **Document Processing**: Automatic handling of various file formats (PDF, DOCX, TXT, etc.)

### Persistent Agents and Threads

Understanding the agent interaction model:

- **Persistent Agents**: AI agents that maintain their configuration and capabilities across sessions
- **Threads**: Conversation contexts that preserve message history and state
- **Runs**: Individual executions of agent processing within a thread
- **Messages**: User inputs and agent responses within a conversation thread


## Description

This challenge is divided into two main tasks that will guide you through creating a simple Travel Policy Compliance Agent solution.

### About the Travel Policy Compliance Agent

You'll build a specialized AI agent that acts as a compliance advisor for company travel policies.

The agent will use the company travel policy document as its knowledge base to provide accurate, policy-compliant guidance to employees.

**Core Capabilities:**

- **Policy Interpretation**: Explain complex policy rules in simple terms
- **Expense Validation**: Check if expenses comply with policy limits
- **Booking Guidance**: Provide recommendations for compliant travel bookings
- **Exception Handling**: Explain when and how to request policy exceptions

### Task 1: Create and Configure the Agent in Azure AI Foundry

Your first task is to set up the AI agent using the Azure AI Foundry portal:

1. **Create an AI Agent in Azure AI Foundry**
   - Navigate to the Azure AI Foundry portal
   - Create a new AI agent with file search capabilities enabled
   - Configure the agent with the following instructions:

   > You are a Travel Compliance Policy Agent for a company. Your role is to review, validate, and enforce the company's travel policy by evaluating travel requests, itineraries, and expense reports. You must ensure all travel activities comply with the policy's rules, financial limits, and approval workflows.

2. **Set Up File Search Knowledge Base**
   - Add file search as a knowledge source for your agent
   - Create a new vector store to hold the travel policy documents
   - Upload the company travel policy document located at `Student\Resources\Challenge-05\company_travel_policy.docx`
   - Ensure the document is properly indexed and searchable

3. **Test in the Playground**
   - Use the Azure AI Foundry playground to test your agent
   - Ask sample questions about travel policies to verify the agent can retrieve relevant information
   - Validate that the agent provides accurate responses based on the uploaded document
   - Test various scenarios like expense limits  and booking requirements

**Sample Test Queries for the Playground:**

- "What is the maximum daily allowance for meals when traveling domestically?"
- "Do I need approval for international flights?"
- "What hotels am I allowed to book?"
- "Can I book first-class flights?"
- "What documents do I need to submit for expense reimbursement?"

### Task 2: Build the Python Application

Your second task is to create a Python application that integrates with your configured agent:

**Application Development:**

1. Create a Python application that:
   - Uses Azure AI Projects SDK for Python
   - Establishes connection with Azure credentials
   - Connects to your configured AI agent from Task 1
   - Manages conversation threads and messages
   - Integrates with file search capabilities

2. Create an interactive console interface:
   - Handle user input for policy questions
   - Display agent responses with proper formatting
   - Provide clear instructions

3. Agent integration:
   - Establish connection to your Azure AI agent
   - Manage conversation threads properly
   - Ensure file search functionality works

**Sample Python Code to Get Started:**

Use this code as a foundation for your application:

`requirements.txt`:

```plaintxt
azure-ai-projects>=0.1.0
azure-identity>=1.25.1
python-dotenv>=1.0.0
```

```python
import asyncio
import os
import sys
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import AgentEventHandler, MessageDeltaChunk
from azure.identity import DefaultAzureCredential


class StreamingEventHandler(AgentEventHandler):
    """Custom event handler to stream agent responses in real-time."""

    def on_message_delta(self, delta: MessageDeltaChunk) -> None:
        """Handle streaming text deltas from the agent."""
        if delta.text:
            print(delta.text, end="", flush=True)


async def run_agent_conversation():
    """Run an interactive conversation with the travel policy agent."""

    # Consider storing secrets in an .env file and loading
    # them using python-dotenv and load_dotenv()
    # See https://pypi.org/project/python-dotenv/.
    endpoint = os.getenv("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT")
    agent_id = os.getenv("AZURE_AI_AGENT_ID")

    if not endpoint or not agent_id:
        print("Error: Set AZURE_AI_FOUNDRY_PROJECT_ENDPOINT and AZURE_AI_AGENT_ID environment variables")
        sys.exit(1)

    project_client = AIProjectClient(endpoint=endpoint, credential=DefaultAzureCredential())

    # Get agents client
    agents_client = project_client.agents

    # Load your agent from Azure AI Foundry
    agent = agents_client.get_agent(agent_id)
    print(f"Loaded agent: {agent.name}")

    # Create a new thread for conversation
    thread = agents_client.threads.create()
    print(f"Created thread: {thread.id}")

    # Interactive conversation loop
    print("\nTravel Policy Compliance Agent")
    print("Ask me about travel policies. Type 'exit' to quit.\n")

    while True:
        # Get user input
        user_input = input("You: ").strip()

        if user_input.lower() == 'exit':
            break

        if not user_input:
            continue

        # Send message to agent
        agents_client.messages.create(
            thread_id=thread.id,
            role="user",
            content=[{"type": "text", "text": user_input}]
        )

        # Stream the agent's response in real-time using custom event handler
        print("Agent: ", end="", flush=True)

        with agents_client.runs.stream(
            thread_id=thread.id,
            agent_id=agent_id,
            event_handler=StreamingEventHandler()
        ) as stream:
            stream.until_done()

        print()  # New line after response

# Main execution
if __name__ == "__main__":
    asyncio.run(run_agent_conversation())
```

**Important Notes:**

- Replace `YOUR_AZURE_AI_FOUNDRY_PROJECT_ENDPOINT` with your actual endpoint from Azure AI Foundry
- Replace `YOUR_AGENT_ID` with the ID of the agent you created in Task 1
- **For faster dependency management, consider using `uv`:** [`uv` is an extremely fast Python package installer and resolver](https://docs.astral.sh/uv/). It's significantly faster than `pip` (10-100x in many cases) and handles dependency resolution more efficiently. You can install it from https://docs.astral.sh/uv/getting-started/installation/.

Install required packages using `uv` (recommended):
```bash
uv pip install azure-ai-projects azure-identity
```

Or using standard `pip`:
```bash
pip install azure-ai-projects azure-identity
```

- The code demonstrates a basic conversation loop - you can extend it with additional features

### What You'll Deliver

After completing both tasks, you will have:

- **A configured AI agent** in Azure AI Foundry with file search capabilities and travel policy knowledge
- **A working console application** that provides an interactive interface to query travel policy information
- **A complete solution** that demonstrates enterprise AI agent capabilities with document-based knowledge retrieval

### Sample Interactions

Your Travel Policy Compliance Agent should be able to handle queries like:

```text
User: "What is the maximum daily allowance for meals when traveling domestically?"
Agent: "According to the company travel policy, the maximum daily meal allowance for domestic travel is $75 per day, which includes breakfast ($15), lunch ($25), and dinner ($35)."

User: "Do I need approval for international flights?"
Agent: "Yes, according to the travel policy, all international travel requires pre-approval from your manager and the travel department at least 2 weeks before departure."

User: "What hotels am I allowed to book?"
Agent: "The travel policy requires you to book accommodations at approved corporate rates when available. For domestic travel, the maximum nightly rate is $200 in major cities and $150 in other locations."
```

## Success Criteria

- ✅ Successfully created and configured an AI agent in Azure AI Foundry with file search capabilities and travel policy knowledge
- ✅ Validated that you uploaded and indexed the travel policy document in a vector store for searchable content
- ✅ Demonstrated agent functionality by testing it in the Azure AI Foundry playground to validate policy-based responses
- ✅ Successfully built a working Python application that connects to your agent and provides an interactive interface
- ✅ Validated policy compliance functionality by ensuring the agent accurately answers travel policy questions

## Learning Resources

### Azure AI Agents Service Documentation

- [Azure AI Agents Service Overview](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/overview)
- [Azure AI Agents Service Concepts](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/concepts/threads-runs-messages)
- [File Search with AI Agents](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/how-to/tools/file-search)

### Sample Code and Tutorials

- [Azure AI Foundry Samples - C#](https://github.com/azure-ai-foundry/foundry-samples/tree/main/samples/microsoft/csharp/getting-started-agents)
- [Azure AI Foundry Samples - Python](https://github.com/azure-ai-foundry/foundry-samples/tree/main/samples/microsoft/python/getting-started-agents)