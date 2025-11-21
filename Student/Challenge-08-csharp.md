# Challenge 08 - C# - Develop Agentic AI Applications using Microsoft Agent Framework and Multi-Agent Architectures
[< Previous Challenge](./Challenge-07-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-09-csharp.md)

[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-08-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-08-python.md)

## Introduction

In this challenge you'll practice how to use the powerful capabilities of Microsoft Agent Framework to design and orchestrate intelligent agents that work collaboratively to solve complex problems. You'll build a multi-agent application that leverages the strengths of different agents to achieve a common goal.

You'll also learn about the different types of orchestration patterns available, and use the Microsoft Agent Framework to develop your own AI agents that can collaborate for a multi-agent solution.

## Key Concepts

The Microsoft Agent Framework's agent orchestration framework makes it possible to design, manage, and scale complex multi-agent workflows without having to manually handle the details of agent coordination. Instead of relying on a single agent to manage every aspect of a task, you can combine multiple specialized agents. Each agent with a unique role or area of expertise can collaborate to create systems that are more robust, adaptive, and capable of solving real-world problems collaboratively.

By orchestrating agents together, you can take on tasks that would be too complex for a single agent—from running parallel analyses, to building multi-stage processing pipelines, to managing dynamic, context-driven handoffs between experts.

### Why multi-agent orchestration matters

Traditional single-agent systems are limited in their ability to handle complex, multi-faceted tasks. By orchestrating multiple agents, each with specialized skills or roles, we can create systems that are more robust, adaptive, and capable of solving real-world problems collaboratively. Multi-agent orchestration in Microsoft Agent Framework provides a flexible foundation for building such systems, supporting a variety of coordination patterns.

### Supported AI Agent orchestration patterns

Like well-known cloud design patterns, agent orchestration patterns are technology agnostic approaches to coordinating multiple agents to work together towards a common goal. To learn more about the patterns themselves, refer to the [AI agent orchestration patterns documentation](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns).

Microsoft Agent Framework supports you by implementing these orchestration patterns directly in the SDK. These patterns are available as part of the framework and can be easily extended or customized so you can tune your agent collaboration scenario.
To learn more about the supported patterns, refer to the [Microsoft Agent Framework Workflows Orchestrations Patterns](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/overview).
| Pattern | Description | Typical Use Case |
|---------|-------------|------------------|
| **Concurrent** | Broadcasts a task to all agents, collects results independently | Parallel analysis, independent subtasks, ensemble decision making |
| **Sequential** | Passes the result from one agent to the next in a defined order | Step-by-step workflows, pipelines, multi-stage processing |
| **Group Chat** | Coordinates multiple agents in a collaborative conversation with a manager controlling speaker selection and flow. | Iterative refinement, collaborative problem-solving, content review. |
| **Handoff** | Dynamically passes control between agents based on context or rules | Dynamic workflows, escalation, fallback, or expert handoff scenarios |

### Agents as tools
"Agents as Tools" is an architectural pattern in AI systems where specialized AI agents are wrapped as callable functions (tools) that can be used by other agents. This creates a hierarchical structure where:

 1. A primary "orchestrator" agent handles user interaction and determines which specialized agent to call
 2. Specialized "tool agents" perform domain-specific tasks when called by the orchestrator

This approach mimics human team dynamics, where a manager coordinates specialists, each bringing unique expertise to solve complex problems. Rather than a single agent trying to handle everything, tasks are delegated to the most appropriate specialized agent.

In some workflows, you may want a central agent to orchestrate a network of specialized agents, instead of handing off control. You can do this by modeling agents as tools.

### A unified orchestration workflow

Regardless of which orchestration pattern you choose, the Microsoft Agent Framework  provides a consistent, developer-friendly interface for building and running them. The typical flow looks like this:

1. Define your agents and describe their capabilities and tools
2. Select and create an orchestration pattern, optionally adding a manager agent if needed
3. Optionally configure callbacks or transforms for custom input and output handling
4. Start a runtime to manage execution
5. Invoke the orchestration with your task
6. Retrieve results in an asynchronous, non-blocking way

Because all patterns share the same core interface, you can easily experiment with different orchestration strategies without rewriting agent logic or learning new APIs. The SDK abstracts the complexity of agent communication, coordination, and result aggregation so you can focus on designing workflows that deliver results.

Multi-agent orchestration in Microsoft Agent Framework provides a flexible, scalable way to build intelligent systems that combine the strengths of multiple specialized agents. With built-in orchestration patterns, a unified development model, and runtime features for managing execution, you can quickly prototype, refine, and deploy collaborative AI workflows. Whether you're running agents in parallel, coordinating sequential steps, or enabling dynamic conversations, the framework gives you the tools to turn multiple agents into a cohesive problem-solving team.

## Prerequisites

### Starting the Travel MCP Server

The Travel MCP Server provides the travel booking APIs (Amadeus) that the agents will use. Make sure it's running before starting this challenge:

Before starting the Travel MCP Server, you need to register for an Amadeus API key:
1. Visit the [Amadeus for Developers portal](https://developers.amadeus.com/)
2. Create an account or sign in
3. Register your application to obtain your API key and secret
4. Configure these credentials in your Travel MCP Server settings

To start the Travel MCP Server, open a terminal and navigate to the Travel MCP Server project directory:

```powershell
# Navigate to the Travel MCP Server directory
cd Student\Resources\Challenge-08\csharp\MCP.Server.Travel.Solution

# Run the server
dotnet run
```

The server should start on `http://localhost:8080` (or the port specified in your configuration).

## Description

In this challenge, you will build a sophisticated multi-agent application using Microsoft Agent Framework. A starter project with pre-built specialized travel agents has been provided [here](./Resources/Challenge-08/csharp/MAF.TravelMultiAgentClient). Your task is to implement orchestration workflows that enable these agents to work together in a multi-turn conversational experience.

### Provided Agents

The starter project includes the following specialized agents, each with specific capabilities powered by Model Context Protocol (MCP) tools:

1. **FlightAgent** - Searches for flights, checks availability, retrieves flight status, and handles flight bookings using Amadeus flight APIs
2. **HotelAgent** - Searches for hotels, checks availability, compares rates, and manages hotel reservations using Amadeus hotel APIs
3. **ActivityAgent** - Recommends activities, attractions, and points of interest based on location and preferences using Amadeus activities APIs
4. **TransferAgent** - Handles ground transportation, airport transfers, and rental car bookings using Amadeus transfer APIs
5. **ReferenceAgent** - Provides reference data such as airport codes, airline information, city details, and travel insights using Amadeus reference APIs
6. **TravelPolicyAgent** - Validates travel plans against company policies using Azure AI Foundry Agent Service (persistent agent with file search)
7. **TravelCoordinatorAgent** - Acts as the main interface with customers and orchestrates the overall travel planning workflow

### Your Task

Your goal is to create **multi-agent orchestration workflows** that enable these agents to collaborate in a natural, multi-turn conversation. You'll implement different orchestration patterns to demonstrate how agents can work together in various ways:

- **Sequential Orchestration** - Process travel requests in a step-by-step pipeline
- **Concurrent Orchestration** - Gather information from multiple agents in parallel
- **Handoff Orchestration** - Enable dynamic handoffs between agents based on context
- **Agents as Tools** - Use specialized agents as callable tools from a main orchestrator

#### Task 1: **Review the Provided Agents**:
   - Examine the pre-built agents in the `MAF.TravelMultiAgentClient/Agents` folder
   - Understand each agent's capabilities and MCP tool integration
   - Review the agent instructions and system prompts

#### Task 2: **Implement Orchestration Patterns**:
Examine and decide which orchestration pattern is more suitable for the given scenario
   - **Sequential Workflow**: Create a pipeline where agents execute one after another in a defined order
   - **Concurrent Workflow**: Implement parallel execution where multiple agents run simultaneously
   - **Handoff Workflow**: Build a dynamic workflow where agents can transfer control to each other based on context
   - **Agents as Tools Pattern**: Create a main orchestrator that uses specialized agents as callable tools

#### Task 3: **Enable Multi-Turn Conversations**:
   - Implement conversation state management to maintain context across turns
   - Allow users to refine their requests based on agent responses
   - Support follow-up questions and iterative planning
   - Maintain conversation history throughout the session

## Success Criteria

To successfully complete this challenge, you must demonstrate:

- ✅ **Understanding of Provided Agents**

- Reviewed all the pre-built agents and understood their capabilities
- Understood how each agent integrates with MCP tools from the Travel MCP Server
- Understood how the TravelPolicyAgent leverages the AI Foundry persistent agent to validate and comply with travel policies
- Can explain the role and purpose of each specialized agent
- Explain the difference between chat client agents and persistent agents (TravelPolicyAgent)

### ✅ **Orchestration Patterns Implementation**

- Examine the Sequential Workflow with proper agent chaining
- Examine the Concurrent Workflow for parallel agent execution
- Examine the Handoff Workflow with dynamic agent transitions
- Examine the Agents as Tools pattern with a main orchestrator
- Demonstrated understanding of when to use each orchestration pattern
- Explain to your coach which orchestration pattern is best suited for the given Travel agents scenario

### ✅ **Multi-Turn Conversation Capability**

- Application maintains conversation context across multiple turns
- Users can ask follow-up questions and refine their requests
- Conversation history is properly managed and passed between agents
- Agents build upon previous responses in the conversation
- Session state is maintained throughout the interaction
### ✅ **Testing and Demonstration**

- Successfully demonstrated the selected orchestration pattern to your coach as a multi-turn conversation
- Provides meaningful example scenarios for travel planning
- Shows how conversation context is maintained across turns
- Demonstrates error handling and graceful failure scenarios

## Learning Resources

### Official Microsoft Documentation

- [Microsoft Agent Framework Workflows Orchestrations](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/overview)
- [Understand Agent Orchestration](https://learn.microsoft.com/en-us/training/modules/orchestrate-semantic-kernel-multi-agent-solution/3-understand-agent-orchestration)
- [AI Agent Orchestration Patterns](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns)
- [Agents in Workflows](https://learn.microsoft.com/en-us/agent-framework/tutorials/workflows/agents-in-workflows?pivots=programming-language-csharp)
- [Amadeus for Developers](https://developers.amadeus.com/)
- [Amadeus Open AI Specification](https://github.com/amadeus4dev/amadeus-open-api-specification/tree/main/spec/json)
