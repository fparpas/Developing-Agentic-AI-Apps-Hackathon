# Challenge 07 - C# - Tracing Intelligence: Observability in Agentic AI with Microsoft Agent Framework

 [< Previous Challenge](./Challenge-06-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-08-csharp.md)
 
[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-07-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-07-python.md)

## Introduction

When you build AI solutions, you want to be able to observe the behavior of your services. Observability is the ability to monitor and analyze the internal state of components within a distributed system. It is a key requirement for building enterprise-ready AI solutions.

## Concepts

Observability is typically achieved through logging, metrics, and tracing. They are often referred to as the three pillars of observability. You will also hear the term "telemetry" used to describe the data collected by these three pillars. Unlike debugging, observability provides an ongoing overview of the system's health and performance.

Microsoft Agent Framework is designed to be observable from the ground up. It emits logs, metrics, and traces that are compatible with the [OpenTelemetry standard](https://opentelemetry.io/docs/specs/semconv/gen-ai/gen-ai-agent-spans/), providing comprehensive insights into agent behavior and performance.

### Observability Features in Microsoft Agent Framework

- **Logging**: Microsoft Agent Framework logs meaningful events and errors from agents, tools, and AI connectors. This includes agent lifecycle events, tool executions, and conversation flows.
- **Metrics**: Microsoft Agent Framework emits metrics from agent operations and AI connectors. You can monitor metrics such as agent response times, tool execution duration, token consumption, and conversation success rates.
- **Tracing**: Microsoft Agent Framework supports distributed tracing with rich context. You can track activities across different agents, tools, and services, providing end-to-end visibility into complex agent workflows.

### Observability Across Agents and Workflows

Microsoft Agent Framework enables observability at multiple levels of your agentic AI applications:

**Agent-Level Observability:**
- **Individual Agent Monitoring**: Track the behavior, performance, and resource consumption of each agent in your system
- **Agent Lifecycle Events**: Monitor agent creation, initialization, activation, and termination
- **Agent-to-Agent Communication**: Observe interactions between multiple agents in collaborative scenarios
- **Agent State Changes**: Track state transitions and context preservation across conversations
- **Tool Usage Patterns**: Monitor which tools agents use most frequently and their success rates

**Workflow-Level Observability:**
- **Workflow Orchestration**: Track the execution flow across multiple agents and services in complex workflows
- **Step-by-Step Execution**: Monitor each step in multi-step workflows, including decision points and branching logic
- **Cross-Workflow Dependencies**: Observe how different workflows interact and depend on each other
- **Workflow Performance Metrics**: Measure end-to-end workflow execution times, success rates, and bottlenecks
- **Resource Utilization**: Monitor compute, memory, and token consumption across entire workflows

This comprehensive observability approach allows you to understand not just individual component performance, but also the emergent behavior of your entire agentic AI system.

You can use the following observability tools to monitor and analyze the behavior of your agents and workflows built with Microsoft Agent Framework:

### Observability Tools

- **Console Exporter**: Although the console is not recommended for production, it provides a simple way to get started with observability during development. Microsoft Agent Framework includes built-in console exporters for quick debugging.
- **Application Insights**: Application Insights is part of Azure Monitor, which is a comprehensive solution for collecting, analyzing, and acting on telemetry data from your cloud and on-premises environments. Perfect for production agent monitoring.
- **Aspire Dashboard**: Aspire Dashboard is part of the .NET Aspire offering. The dashboard allows developers to monitor and inspect their distributed applications, including multi-agent scenarios.
- **Azure AI Foundry Tracing UI**: Azure AI Foundry provides specialized tracing capabilities for AI applications, offering detailed insights into agent conversations, tool usage, and model interactions.
- **OpenTelemetry Compatible Tools**: Since Microsoft Agent Framework uses OpenTelemetry standards, you can integrate with any OpenTelemetry-compatible observability platform like Grafana.

## Description
You should incorporate observability into your Microsoft Agent Framework application using a Console exporter and one or more of the following approaches to visualize and analyze the telemetry data:

1. Console Exporter for development and debugging
2. Application Insights for production monitoring
3. Aspire Dashboard for distributed application insights

Use the Agent Framework application created in the previous challenge and add comprehensive observability to track agent behavior, tool executions, and conversation flows.

## Success Criteria
- ✅ Ensure that your Agent Framework application is running with observability enabled
- ✅ See the traces generated with Console exporter
- ✅ Visualize traces using at least one of the recommended tools (Application Insights or Aspire Dashboard, )
- ✅ Inspect the telemetry data and observe the sequence of agent operations, tool calls, and AI model interactions
- ✅ Demonstrate that you can see agent conversation history, tool execution details, and performance metrics
- ✅ Show how observability helps in debugging and monitoring agent behavior in real-time

## Learning Resources

### Agents Observability
- [Agent Observability Overview](https://learn.microsoft.com/en-us/agent-framework/user-guide/agents/agent-observability?pivots=programming-language-csharp)
- [Tutorial - Enabling observability for Agents](https://learn.microsoft.com/en-us/agent-framework/tutorials/agents/enable-observability?pivots=programming-language-csharp)
- [Sample - Agent Observability with Console Exporter](https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/GettingStarted/Agents/Agent_Step08_Observability)
- [Sample - OpenTelemetry with the Microsoft Agent Framework](https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/GettingStarted/AgentOpenTelemetry)
- [Microsoft Agent Framework Workflows - Observability](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/observability)
- [View trace results for AI applications using OpenAI SDK](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/trace-application)