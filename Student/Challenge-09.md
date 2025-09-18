# Challenge 09 - Develop Agentic AI Applications using Semantic Kernel and Multi-Agent Architectures

[< Previous Challenge](./Challenge-08.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-10.md)

## Introduction

In this challenge you'll practice how to use the powerful capabilities of Semantic Kernel to design and orchestrate intelligent agents that work collaboratively to solve complex problems. You'll build a multi-agent application that leverages the strengths of different agents to achieve a common goal.

You'll also learn about the different types of orchestration patterns available, and use the Semantic Kernel Agents Framework to develop your own AI agents that can collaborate for a multi-agent solution.

## Key Concepts

The Semantic Kernel SDK's agent orchestration framework makes it possible to design, manage, and scale complex multi-agent workflows without having to manually handle the details of agent coordination. Instead of relying on a single agent to manage every aspect of a task, you can combine multiple specialized agents. Each agent with a unique role or area of expertise can collaborate to create systems that are more robust, adaptive, and capable of solving real-world problems collaboratively.

By orchestrating agents together, you can take on tasks that would be too complex for a single agent—from running parallel analyses, to building multi-stage processing pipelines, to managing dynamic, context-driven handoffs between experts.

### Why multi-agent orchestration matters

Traditional single-agent systems are limited in their ability to handle complex, multi-faceted tasks. By orchestrating multiple agents, each with specialized skills or roles, we can create systems that are more robust, adaptive, and capable of solving real-world problems collaboratively. Multi-agent orchestration in Semantic Kernel provides a flexible foundation for building such systems, supporting a variety of coordination patterns.

### Supported orchestration patterns

Like well-known cloud design patterns, agent orchestration patterns are technology agnostic approaches to coordinating multiple agents to work together towards a common goal. To learn more about the patterns themselves, refer to the AI agent orchestration patterns documentation.

Semantic Kernel supports you by implementing these orchestration patterns directly in the SDK. These patterns are available as part of the framework and can be easily extended or customized so you can tune your agent collaboration scenario.

| Pattern | Description | Typical Use Case |
|---------|-------------|------------------|
| **Concurrent** | Broadcasts a task to all agents, collects results independently | Parallel analysis, independent subtasks, ensemble decision making |
| **Sequential** | Passes the result from one agent to the next in a defined order | Step-by-step workflows, pipelines, multi-stage processing |
| **Handoff** | Dynamically passes control between agents based on context or rules | Dynamic workflows, escalation, fallback, or expert handoff scenarios |
| **Group Chat** | All agents participate in a group conversation, coordinated by a group manager | Brainstorming, collaborative problem solving, consensus building |
| **Magentic** | Group chat-like orchestration inspired by MagenticOne research | Complex, generalist multi-agent collaboration |

### A unified orchestration workflow

Regardless of which orchestration pattern you choose, the Semantic Kernel SDK provides a consistent, developer-friendly interface for building and running them. The typical flow looks like this:

1. Define your agents and describe their capabilities
2. Select and create an orchestration pattern, optionally adding a manager agent if needed
3. Optionally configure callbacks or transforms for custom input and output handling
4. Start a runtime to manage execution
5. Invoke the orchestration with your task
6. Retrieve results in an asynchronous, non-blocking way

Because all patterns share the same core interface, you can easily experiment with different orchestration strategies without rewriting agent logic or learning new APIs. The SDK abstracts the complexity of agent communication, coordination, and result aggregation so you can focus on designing workflows that deliver results.

Multi-agent orchestration in the Semantic Kernel SDK provides a flexible, scalable way to build intelligent systems that combine the strengths of multiple specialized agents. With built-in orchestration patterns, a unified development model, and runtime features for managing execution, you can quickly prototype, refine, and deploy collaborative AI workflows. Whether you’re running agents in parallel, coordinating sequential steps, or enabling dynamic conversations, the framework gives you the tools to turn multiple agents into a cohesive problem-solving team.

## Description

In this challenge, you will build a sophisticated multi-agent application using Semantic Kernel's Agent Orchestration framework. You'll create specialized agents that work together to solve a complex business scenario requiring multiple areas of expertise.

Your task is to develop a **Travel Planning Assistant** that uses multiple agents to collaboratively plan a comprehensive trip. This scenario requires:

1. **Weather Agent** - Provides weather forecasts and recommendations
2. **Budget Agent** - Calculates costs and suggests budget-friendly alternatives
3. **Activity Agent** - Recommends activities and attractions based on interests
4. **Restaurant Agent** - Suggests dining options based on preferences and budget
5. **Coordinator Agent** - Orchestrates the collaboration and provides final recommendations

You'll implement different orchestration patterns to demonstrate how agents can work together in various ways:

- Use **Sequential Orchestration** for the main planning pipeline
- Use **Concurrent Orchestration** for gathering parallel information
- Use **Group Chat Orchestration** for collaborative decision-making scenarios

### Requirements

1. **Set up the Semantic Kernel Agent Framework**:
   - Install the required NuGet packages for agent orchestration
   - Configure the agent runtime environment

2. **Create Specialized Agents**:
   - Implement each agent with specific instructions and capabilities
   - Configure appropriate AI models for each agent's role
   - Define clear interfaces and responsibilities

3. **Implement Orchestration Patterns**:
   - **Sequential Pattern**: Chain agents for step-by-step planning
   - **Concurrent Pattern**: Run multiple agents in parallel for information gathering
   - **Group Chat Pattern**: Enable collaborative discussion between agents

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

- [Semantic Kernel Agent Orchestration](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-orchestration/?pivots=programming-language-csharp)
- [Understand Agent Orchestration](https://learn.microsoft.com/en-us/training/modules/orchestrate-semantic-kernel-multi-agent-solution/3-understand-agent-orchestration)
- [AI Agent Orchestration Patterns](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns)
- [Semantic Kernel Agents Framework](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/)

### Additional Resources

- [MagenticOne Research Paper](https://www.microsoft.com/en-us/research/articles/magentic-one-a-generalist-multi-agent-system-for-solving-complex-tasks/)
- [Semantic Kernel GitHub Repository](https://github.com/microsoft/semantic-kernel)
- [Multi-Agent Systems Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/ai-agent-design-patterns)