![](https://img.shields.io/badge/For%20Final%20Review-orange)
![](https://img.shields.io/badge/Collect%20Feedback-orange)

# Challenge 08 - C# - Develop Agentic AI Applications using Microsoft Agent Framework and Multi-Agent Architectures

[< Previous Challenge](./Challenge-07-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-09-csharp.md)

[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-08-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-08-python.md)

![](https://img.shields.io/badge/Challenge%20Under%20Development-red)

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
| **Handoff** | Dynamically passes control between agents based on context or rules | Dynamic workflows, escalation, fallback, or expert handoff scenarios |
   | **Magentic** | Group chat-like orchestration inspired by MagenticOne research | Complex, generalist multi-agent collaboration |

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

Multi-agent orchestration in Microsoft Agent Framework provides a flexible, scalable way to build intelligent systems that combine the strengths of multiple specialized agents. With built-in orchestration patterns, a unified development model, and runtime features for managing execution, you can quickly prototype, refine, and deploy collaborative AI workflows. Whether you’re running agents in parallel, coordinating sequential steps, or enabling dynamic conversations, the framework gives you the tools to turn multiple agents into a cohesive problem-solving team.

## Description

In this challenge, you will build a sophisticated multi-agent application using Microsoft Agent Framework. You'll create specialized agents that work together to solve a complex business scenario requiring multiple areas of expertise.

Your task is to develop a **Travel Planning Assistant** that uses multiple agents to collaboratively plan a comprehensive trip. This scenario requires:

1. **Search Flights Agent** - Provides flight options and recommendations
2. **Search Hotels Agent** - Provides hotel options and recommendations
3. **Activity Agent** - Recommends activities and attractions based on interests
4. **Travel Policy Compliance Agent** - Check if travelling complies with the company policies
4. **Coordinator Agent** - Orchestrates the collaboration and provides final recommendations

You'll implement different orchestration patterns to demonstrate how agents can work together in various ways:

- Use **Sequential Orchestration** for the main planning pipeline
- Use **Concurrent Orchestration** for gathering parallel information

### Requirements

1. **Set up the Microsoft Agent Framework**:
   - Install the required NuGet packages for agent orchestration
   - Configure the agent runtime environment

2. **Create Specialized Agents**:
   - Implement each agent with specific instructions and capabilities
   - Configure appropriate AI models for each agent's role
   - Define clear interfaces and responsibilities

3. **Implement Orchestration Patterns**:
   - **Sequential Pattern**: Chain agents for step-by-step planning
   - **Concurrent Pattern**: Run multiple agents in parallel for information gathering

4. **Build the Travel Planning Workflow**:
   - Accept user input for destination, dates, budget, and preferences
   - Coordinate agents to gather and process relevant information
   - Generate a comprehensive travel plan with recommendations

5. **Add Error Handling and Fallbacks**:
   - Implement proper error handling for agent failures
   - Add fallback mechanisms for when agents cannot complete tasks
   - Ensure graceful degradation of functionality

## Success Criteria

To successfully complete this challenge, you must demonstrate:

### ✅ **Agent Implementation**

- [ ] Created 5 specialized agents with distinct roles and capabilities
- [ ] Each agent has appropriate system instructions and prompts
- [ ] Agents are properly configured with AI models and tools
- [ ] Agent responses are contextually appropriate for their roles

### ✅ **Orchestration Patterns**

- [ ] Implemented Sequential Orchestration for the main workflow
- [ ] Implemented Concurrent Orchestration for parallel information gathering
- [ ] Implemented Group Chat Orchestration for collaborative scenarios
- [ ] Demonstrated proper runtime management and cleanup

### ✅ **Travel Planning Functionality**

- [ ] Application accepts user input for travel requirements
- [ ] Weather agent provides relevant forecast and recommendations
- [ ] Budget agent calculates costs and suggests alternatives
- [ ] Activity agent recommends relevant attractions and activities
- [ ] Restaurant agent suggests appropriate dining options
- [ ] Coordinator agent synthesizes all information into a coherent plan

### ✅ **Code Quality and Architecture**

- [ ] Clean, well-structured code with proper separation of concerns
- [ ] Appropriate error handling and logging
- [ ] Proper async/await patterns for agent coordination
- [ ] Configuration management for API keys and settings

### ✅ **Documentation**

- [ ] Clear README with setup and usage instructions
- [ ] Code comments explaining orchestration choices
- [ ] Example scenarios and expected outputs

## Learning Resources

### Official Microsoft Documentation

- [Microsoft Agent Framework Workflows Orchestrations](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/overview)
- [Understand Agent Orchestration](https://learn.microsoft.com/en-us/training/modules/orchestrate-semantic-kernel-multi-agent-solution/3-understand-agent-orchestration)
- [AI Agent Orchestration Patterns](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns)
- [Agents in Workflows](https://learn.microsoft.com/en-us/agent-framework/tutorials/workflows/agents-in-workflows?pivots=programming-language-csharp)
- [Amadeus for Developers](https://developers.amadeus.com/)
- [Amadeus Open AI Specification](https://github.com/amadeus4dev/amadeus-open-api-specification/tree/main/spec/json)