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

**Chat Client Observability:**
- **User Interaction Tracing**: Capture and analyze user messages, commands, and feedback within the chat client.
- **Conversation Flow Monitoring**: Observe the sequence of messages exchanged between users and agents, including context switches and interruptions.
- **Latency and Response Metrics**: Measure the time taken for agents to respond to user inputs and overall chat responsiveness.
- **Error and Exception Logging**: Track errors, failed responses, and unexpected behaviors in the chat interface.
- **User Experience Insights**: Collect telemetry on user engagement, satisfaction, and common issues to improve the chat client experience.

**Agent-Level Observability:**
- **Individual Agent Monitoring**: Track the behavior, performance, and resource consumption of each agent in your system.
- **Agent Lifecycle Events**: Monitor agent creation, initialization, activation, and termination.
- **Agent-to-Agent Communication**: Observe interactions between multiple agents in collaborative scenarios.
- **Agent State Changes**: Track state transitions and context preservation across conversations.
- **Tool Usage Patterns**: Monitor which tools agents use most frequently and their success rates.

**Workflow-Level Observability:**
- **Workflow Orchestration**: Track the execution flow across multiple agents and services in complex workflows.
- **Step-by-Step Execution**: Monitor each step in multi-step workflows, including decision points and branching logic.
- **Cross-Workflow Dependencies**: Observe how different workflows interact and depend on each other.
- **Workflow Performance Metrics**: Measure end-to-end workflow execution times, success rates, and bottlenecks.
- **Resource Utilization**: Monitor compute, memory, and token consumption across entire workflows.

This comprehensive observability approach allows you to understand not just individual component performance, but also the emergent behavior of your entire agentic AI system.

You can use the following observability tools to monitor and analyze the behavior of your agents and workflows built with Microsoft Agent Framework:

### Observability Tools

- **Console Exporter**: Although the console is not recommended for production, it provides a simple way to get started with observability during development. Microsoft Agent Framework includes built-in console exporters for quick debugging.
- **Trace locally with AI Toolkit**: AI Toolkit offers a simple way to trace locally in VS Code. It uses a local OTLP-compatible collector, making it perfect for development and debugging without needing cloud access. The toolkit supports the OpenAI SDK and other AI frameworks through OpenTelemetry. You can see traces instantly in your development environment.
- **Application Insights**: Application Insights is part of Azure Monitor, which is a comprehensive solution for collecting, analyzing, and acting on telemetry data from your cloud and on-premises environments. Perfect for production agent monitoring.
- **Aspire Dashboard**: Aspire Dashboard is part of the .NET Aspire offering. The dashboard allows developers to monitor and inspect their distributed applications, including multi-agent scenarios.
- **Microsoft Foundry Tracing UI**: Microsoft Foundry provides specialized tracing capabilities for AI applications, offering detailed insights into agent conversations, tool usage, and model interactions.
- **OpenTelemetry Compatible Tools**: Since Microsoft Agent Framework uses OpenTelemetry standards, you can integrate with any OpenTelemetry-compatible observability platform like Grafana.

## Description
You should incorporate observability into your Microsoft Agent Framework application using a Console exporter and Application Insights to visualize and analyze the telemetry data

Use the Agent Framework application created in the previous challenge and add comprehensive observability to track agent behavior, tool executions, and conversation flows.

### Task 1: Enable Observability with Console Exporter
The Console Exporter is the quickest way to get started with observability. It prints traces, metrics, and logs directly to the terminal,  ideal for local development and debugging before wiring up a cloud backend.

Enable the Console Exporter in your application to see real-time telemetry data as you interact with your agents. This will help you understand the sequence of operations, identify any errors, and gain insights into agent behavior during development.

### Task 2: Enable Observability with Application Insights
Application Insights (part of Azure Monitor) is the recommended observability backend for production workloads. It stores traces, metrics, and logs in the cloud, and surfaces them through the Azure Portal and Microsoft Foundry Tracing UI.

Enable Application Insights in your Microsoft Agent Framework application to collect telemetry data in a production-like environment. This will allow you to monitor your agents' performance, track tool usage, and analyze conversation flows over time.

### Task 3: Analyze Telemetry Data and Visualize Traces in Microsoft Foundry Tracing UI
Microsoft Foundry provides a purpose-built tracing experience for AI applications that surfaces the agent conversation, individual tool invocations, model requests/responses, token counts, and latency — all in a single timeline view.

Enable and use the Microsoft Foundry Tracing UI to visualize the telemetry data collected from your agents. Analyze the traces to understand the sequence of agent operations, identify any bottlenecks or errors, and gain insights into how your agents are interacting with tools and AI models.

## Success Criteria
- ✅ Add the Console Exporter to your Agent Framework application
- ✅ Run the application and observe traces, metrics, and logs printed to the terminal in real-time

- ✅ Configure Application Insights in your Agent Framework application using a valid connection string
- ✅ Verify that telemetry data (traces, metrics, and logs) is being sent to and collected in Application Insights

- ✅ Open the Microsoft Foundry Tracing UI and locate the traces generated by your application
- ✅ Inspect the trace timeline to observe the sequence of agent operations, tool calls, and AI model interactions
- ✅ Demonstrate that you can see agent conversation history, tool execution details, and performance metrics in the trace view

## Learning Resources

### Agents Observability
- [Microsoft Agent Framework Agent - Observability Overview](https://learn.microsoft.com/en-us/agent-framework/agents/observability?pivots=programming-language-csharp)
- [Microsoft Agent Framework Workflows - Observability](https://learn.microsoft.com/en-us/agent-framework/workflows/observability?pivots=programming-language-csharp)
- [VS Code - Tracing in AI Toolkit](https://code.visualstudio.com/docs/intelligentapps/tracing)
- [Sample - OpenTelemetry with the Microsoft Agent Framework](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/AgentOpenTelemetry)

#### Microsoft Foundry Observability
- [Agent tracing overview](https://learn.microsoft.com/en-us/azure/ai-foundry/observability/concepts/trace-agent-concept?view=foundry)
- [Set up tracing in Microsoft Foundry](https://learn.microsoft.com/en-us/azure/ai-foundry/observability/how-to/trace-agent-setup?view=foundry)