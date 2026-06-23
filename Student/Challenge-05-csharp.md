# Challenge 05 - C# - Build your first AI Agent with AI Agents Service

 [< Previous Challenge](./Challenge-04-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-06-csharp.md)
 
[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-05-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-05-python.md)

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

### Agent Conversations and Responses

Understanding the agent interaction model:

- **Agents**: AI agents configured with instructions, tools, and model deployments in Azure AI Foundry
- **Conversations**: Managed conversation contexts that preserve message history and state across interactions
- **Responses**: Agent replies generated via the Responses API, supporting streaming and structured output
- **ProjectResponsesClient**: The client used to send user messages and receive agent responses within a conversation

## Description

This challenge is divided into two main tasks that will guide you through creating a simple Travel Policy Compliance Agent solution.

### About the Travel Policy Compliance Agent

You'll build a specialized AI agent that acts as a compliance advisor for company travel policies. 

The agent will use the company travel policy document as its knowledge base to provide accurate, policy-compliant guidance to employees.

### Task 1: Create and Configure the Agent in Azure AI Foundry

Your first task is to set up the AI agent using the Azure AI Foundry portal:

1. **Create an AI Agent in Azure AI Foundry**
   - Navigate to the Azure AI Foundry portal
   - Create a new AI agent with file search capabilities enabled
   - Configure the agent with the following instructions:

   ```text
   You are a Travel Compliance Policy Agent for a company. Your role is to review, validate, and enforce the company's travel policy by evaluating travel requests, itineraries, and expense reports. You must ensure all travel activities comply with the policy's rules, financial limits, and approval workflows.
   ```

2. **Set Up File Search Knowledge Base**
   - Add file search as a knowledge source for your agent
   - Create a new vector store to hold the travel policy documents
   - Upload the company travel policy document located [here](../Student/Resources/Challenge-05/company_travel_policy.docx)
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

### Task 2: Create an Agent-Level Guardrail

Add a guardrail at the agent level to protect against sensitive data exposure in the agent's interactions:

1. **Create a New Guardrail**
   - In Azure AI Foundry, create a new guardrail and apply it at the agent level
   - Leave all default values as they are

2. **Enable PII Sensitive Data Leakage Protection**
   - Enable **PII (sensitive data) leakage** protection
   - Within the PII protection settings, enable **email** protection

3. **Validate the Guardrail**
   - Test the guardrail in the Azure AI Foundry playground using a prompt that contains an email address
   - Confirm that the guardrail detects and blocks/masks the email

   **Sample Validation Prompt:**

   ```text
   My email is john.doe@contoso.com. Can you confirm my domestic meal allowance?
   ```

### Task 3: Build the Console Application

Your next task is to create a C# console application that integrates with your configured agent:

**Project Starter Available:**
A starter project is provided [here](../Student/Resources/Challenge-05/csharp/AgentService.FileSearch/Program.cs) to help you get started. However, you'll need to complete the code implementation to establish the conversation flow with your Agent created in previous task

1. **Console Application Development**
   - Use the provided starter project or build a C# console application from scratch using the Azure AI Projects SDK
   - Implement proper authentication using Azure credentials
   - Connect to your configured AI agent from previous task
   - Complete the code to enable full conversation functionality

2. **Interactive Interface Implementation**
   - Create an interactive conversation interface
   - Handle user input and display agent responses

3. **Agent Integration**
   - Establish connection to your Azure AI agent
   - Manage conversation threads and message handling
   - Ensure the console app can access the file search capabilities you configured

**Sample C# Code to Get Started:**

Use this code as a foundation for your console application. Remember to replace the endpoint URL and agent name with your actual values from previous task.

**💡 Tip:** You can also find similar code samples by clicking the **"View Code"** button in the Azure AI Foundry Agent playground after testing your agent.

```csharp
using Azure;
using Azure.Identity;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Extensions.OpenAI;
using OpenAI.Responses;

async Task RunAgentConversation()
{
   // Retrieve configuration values for Azure AI Foundry endpoint and agent name
   var microsoftFoundryEndpoint = configuration["AIAgentService:MicrosoftFoundryEndpoint"];
   var agentName = configuration["AIAgentService:AgentName"];

   // Initialize the AI Project client with Azure credentials for authentication
   AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

   // Optional Step: Create a conversation to use with the agent
   // This maintains the conversation context and message history
   ProjectConversation conversation = projectClient.ProjectOpenAIClient.GetProjectConversationsClient().CreateProjectConversation();

   // Get the response client for the agent and conversation
   // This client is used to send messages and retrieve responses from the agent
   ProjectResponsesClient responsesClient = projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(
      defaultAgent: agentName,
      defaultConversationId: conversation.Id);

    // Send a message to the agent and receive the response
    // The agent will search the file knowledge base and return relevant information
    ResponseResult response = responsesClient.CreateResponse("Enter your prompt here");
    
    // Display the agent's response to the console
    Console.Write(response.GetOutputText());
}

// Main execution
await RunAgentConversation();
```

**Important Notes:**

- Replace the endpoint URL with your Azure AI Foundry project endpoint
- Replace the agent name with the name of the agent you created in Task 1
- This code demonstrates a single conversation - extend it to create an interactive loop for continuous user input

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
- ✅ Created an agent-level guardrail with default values, with PII (sensitive data) leakage protection enabled, including email protection
- ✅ Successfully built a working console application that connects to your agent and provides an interactive interface
- ✅ Validated policy compliance functionality by ensuring the agent accurately answers travel policy questions

## Learning Resources

- [Azure AI Agents Service Overview](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/overview)
- [File Search with AI Agents](https://learn.microsoft.com/en-us/azure/ai-foundry/agents/how-to/tools/file-search)
- [Microsoft Foundry Quickstart](https://learn.microsoft.com/en-us/azure/ai-foundry/quickstarts/get-started-code?view=foundry&preserve-view=true&tabs=csharp)
- [Guardrails and controls overview in Microsoft Foundry](https://learn.microsoft.com/en-us/azure/foundry/guardrails/guardrails-overview?view=foundry)