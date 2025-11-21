# Developing Agentic AI Apps Hackathon

## Introduction

Welcome to this hands‑on hackathon for building Agentic AI applications. You’ll design, build, secure, observe, orchestrate, and scale intelligent AI solutions on Azure.

This hackathon provides a deep‑dive experience for developers building Agentic AI applications.

The hackathon is a collaborative learning experience, structured as a set of challenges to practice technical skills. By participating, you will gain a clear understanding of the capabilities of Agentic AI applications.

Over a progressive series of challenges, you will deepen your understanding of Agentic architectures and Azure services. Each challenge introduces new concepts and capabilities, building on previous outcomes to reinforce architectural patterns while expanding your knowledge of Azure and Agentic technologies.

This workshop typically requires two full days to complete, depending on participants' skill levels. It is a collaborative activity where participants form teams of 3–5 to work through each challenge.

All technical tracks are offered in both C# and Python, allowing you to choose your preferred language for each challenge. The repository includes fully guided student instructions and coach solutions for reference.

## Learning Objectives
Upon completing the workshop, participants will be able to:
- Understand and implement Model Context Protocol (MCP) servers and clients for enhanced AI tool integration
- Build and deploy intelligent applications using Microsoft Agent Framework
- Create and manage AI Agents using Azure AI Agents Service with file search capabilities
- Develop Agentic applications with multi-agent architectures and orchestration patterns
- Implement secure remote MCP servers with proper authentication and deployment strategies
- Build advanced Agentic RAG (Retrieval‑Augmented Generation) systems using Azure AI Search
- Apply observability and tracing techniques to monitor AI application behavior
- Integrate Azure AI services to create comprehensive intelligent solutions
- Secure access to MCP servers with Azure API Management authentication and policies
- Transform existing REST APIs into MCP servers through Azure API Management
- Implement MCP server registration and discovery using Azure API Center for centralized governance
- Apply learned concepts to create innovative solutions that address real‑world challenges across industries

## Prerequisites
- Familiarity with Azure services and the Azure portal
- Good understanding of AI and generative models
- Experience programming with .NET (C#) or Python
- Basic knowledge of REST APIs and web development concepts
- Your laptop (development machine): Windows, macOS or Linux with **administrator rights**
- Active Azure Subscription with **Owner access** to create, modify resources and manage role assignments
- Access to Azure OpenAI in the desired Azure subscription
- GitHub Copilot license (or a one‑time Copilot Pro trial, valid for 30 days)
- Latest version of Azure CLI installed
- For C#/.NET track:
  - Latest version of Visual Studio Code
  - .NET 8.0 SDK or later version
- For Python track:
  - Latest version of Visual Studio Code with the [Python extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python) and [Jupyter package](https://pypi.org/project/jupyter/)
  - Python 3.12 or later

## Target Audience

The intended audience are individuals with coding skills.

- AI Engineers
- Software Developers
- Solution Architects

## Challenges

---
 

All hands-on challenges (from Challenge 2 onward) are available in both C# and Python.

### Challenge 0: **Set up and Prepare the Environment** ([Challenge Guide](Student/Challenge-00.md))

- Install the required development tools. This initial session ensures all participants are prepared and can fully engage with the workshop content.

### Challenge 1: **Accelerate Developer Productivity with MCP Servers in Visual Studio Code** ([Challenge Guide](Student/Challenge-01.md))

- Learn how to boost development productivity by integrating Model Context Protocol (MCP) servers directly into Visual Studio Code, enabling enhanced AI‑powered development workflows.

 
### Challenge 2: **Build Your First MCP Server**  ([C#](Student/Challenge-02-csharp.md) | [Python](Student/Challenge-02-python.md))

- Create your first Model Context Protocol server from scratch. Learn MCP architecture fundamentals and build a weather server that exposes tools and resources over standard transport protocols.

### Challenge 3: **Build Your First MCP Client**  ([C#](Student/Challenge-03-csharp.md) | [Python](Student/Challenge-03-python.md))

- Develop an MCP client using C#/.NET or Python that connects to MCP servers, discovers their capabilities, and interacts with tools through an AI assistant interface.

### Challenge 4: **Host MCP Remote Servers on ACA or Azure Functions**  ([C#](Student/Challenge-04-csharp.md) | [Python](Student/Challenge-04-python.md))

- Deploy your MCP server to the cloud using Azure Container Apps (ACA) or Azure Functions, transforming local development tools into scalable, remotely accessible services.

 
### Challenge 5: **Build Your First AI Agent with Azure AI Agents Service**  ([C#](Student/Challenge-05-csharp.md) | [Python](Student/Challenge-05-python.md))

- Create a sophisticated AI agent using Azure AI Agents Service with file search capabilities. Build a Travel Policy Compliance Agent that analyzes and answers questions using intelligent document retrieval.

### Challenge 6: **Build Your First Microsoft Agent Framework App and Integrate with a Remote MCP Server**  ([C#](Student/Challenge-06-csharp.md) | [Python](Student/Challenge-06-python.md))

- Develop your first intelligent application using Microsoft Agent Framework and integrate it with a remote MCP server to create an AI assistant that leverages external tools and capabilities.

### Challenge 7: **Tracing Intelligence: Observability in Agentic AI with Microsoft Agent Framework**  ([C#](Student/Challenge-07-csharp.md) | [Python](Student/Challenge-07-python.md))

- Enable OpenTelemetry‑based observability (console exporter + Application Insights or Aspire Dashboard) for your Agent Framework app to trace conversations, tool calls, model usage, and diagnose performance.

### Challenge 8: **Develop Agentic AI Applications Using Microsoft Agent Framework and Multi‑Agent Architectures**  ([C#](Student/Challenge-08-csharp.md) | [Python](Student/Challenge-08-python.md))

- Build a multi‑agent “Travel Planning Assistant” using Microsoft Agent Framework orchestration patterns (sequential, concurrent, handoff, agents as tools) combining specialized agents (flights, hotels, activities, policy, coordinator) into a cohesive plan.

### Challenge 9: **Secure Your MCP Remote Server Using an API Key**  ([C#](Student/Challenge-09-csharp.md) | [Python](Student/Challenge-09-python.md))

- Enhance MCP server security by implementing API key authentication or integrate with Entra ID as the identity provider. Secure remote MCP servers while enabling safe access from multiple clients over the internet.

### Challenge 10: **Build Agentic RAG with Azure AI Search**  ([C#](Student/Challenge-10-csharp.md) | [Python](Student/Challenge-10-python.md))

- Create an advanced agentic Retrieval‑Augmented Generation system using Azure AI Search. Build intelligent agents that dynamically decide what information to retrieve and how to synthesize comprehensive responses.

### Challenge 11: **Optional – Secure Access to MCP Servers in API Management** ([Challenge Guide](Student/Challenge-11.md))

- Learn how to secure access to Model Context Protocol servers using Azure API Management by implementing authentication, rate limiting, and other security policies to protect MCP endpoints.

### Challenge 12: **Optional – Expose a REST API in API Management as an MCP Server** ([Challenge Guide](Student/Challenge-12.md))

- Transform existing REST APIs into Model Context Protocol servers through Azure API Management, enabling seamless integration of traditional APIs into MCP‑based agentic workflows.

### Challenge 13: **Optional – Register and Discover Remote MCP Servers in Your API Inventory** ([Challenge Guide](Student/Challenge-13.md))

- Implement MCP server registration and discovery mechanisms in your API inventory using Azure API Center, enabling centralized management and governance of distributed MCP servers.

## References

- [Microsoft Learn - Develop AI agents on Azure](https://learn.microsoft.com/en-us/training/paths/develop-ai-agents-on-azure/)
- [Microsoft GitHub Repo - AI Agents for Beginners](https://github.com/microsoft/ai-agents-for-beginners)
- [Build your code-first agent with Azure AI Foundry](https://microsoft.github.io/build-your-first-agent-with-azure-ai-agent-service-workshop/lab-1-function_calling/)
- [Model Context Protocol (MCP): Integrating Azure OpenAI for Enhanced Tool Integration and Prompting](https://techcommunity.microsoft.com/blog/azure-ai-services-blog/model-context-protocol-mcp-integrating-azure-openai-for-enhanced-tool-integratio/4393788)
- [Microsoft GitHub Repo - MCP for beginners](https://github.com/microsoft/mcp-for-beginners)
- [MCP GitHub Repo](https://github.com/modelcontextprotocol)
- [MCP - Model Context Protocol](https://modelcontextprotocol.io/docs/getting-started/intro)
- [Remote MCP Servers](https://mcpservers.org/remote-mcp-servers)
- [MCP GitHub Repo - MCP Servers](https://github.com/modelcontextprotocol/servers)
- [VS Code - MCP Servers](https://code.visualstudio.com/mcp)
- [Microsoft Learn - Microsoft Agent Framework Overview](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Microsoft Learn - Develop AI agents on Azure OpenAI and Microsoft Agent Framework](https://learn.microsoft.com/en-us/training/paths/develop-ai-agents-azure-open-ai-semantic-kernel-sdk/)
- [Full Course (Lessons 1-11) MCP for Beginners](https://www.youtube.com/watch?v=VfZlglOWWZw)
- [Microsoft GitHub repo - MCP servers](https://github.com/Microsoft/mcp)

## Repository Contents

- `./Student`
  - Student challenge guides
- `./Student/Resources`
  - Resource files, sample code, scripts, etc. (Packaged by the coach and provided to students at the start of the event)
- `./Coach`
  - Coach guide and related files
- `./Coach/Solutions`
  - Solution files with completed example answers to challenges

## Remarks

- Please note that the content of this workshop may become outdated, as Azure AI is a rapidly evolving platform. We recommend staying engaged with the AI community for the most current updates and practices.
    
## Contributors

- Phanis Parpas
- Adrian Calinescu
