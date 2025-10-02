# Developing Agentic AI Apps Hackathon

This workshop requires two full days to finish depending on the attendees' skill level. It is a collaborative activity where attendees form teams of 3-5 people to go through every workshop.
  
## Learning Objectives
Upon completing the workshop, participants will be able to:
- Understand and implement Model Context Protocol (MCP) servers and clients for enhanced AI tool integration
- Build and deploy intelligent applications using Semantic Kernel framework in C# and .NET
- Create and manage AI Agents using Azure AI Agents Service with file search capabilities
- Develop agentic applications with multi-agent architectures and orchestration patterns
- Implement secure remote MCP servers with proper authentication and deployment strategies
- Build advanced Agentic RAG (Retrieval-Augmented Generation) systems using Azure AI Search
- Apply observability and tracing techniques for monitoring AI application behavior
- Integrate various Azure AI services to create comprehensive intelligent solutions
- Apply the learned concepts to create innovative solutions that address real-world challenges across industries.

## Prerequisites
- Familiarity with Azure services and the Azure portal
- Good understanding of AI and generative models
- Experience in programming with C# and .NET
- Basic knowledge of REST APIs and web development concepts
- Your laptop (development machine): Windows, macOS or Linux with **administrator rights**
- Active Azure Subscription with **Contributor access** to create or modify resources
- Access to Azure OpenAI in the desired Azure subscription
- Latest version of Azure CLI
- Latest version of Visual Studio Code or Visual Studio
- .NET 8.0 SDK or later version

## Target Audience
The intended audience are individuals with coding skills.
- AI Engineers
- Software Developers
- Solution Architects

## Challenges

---

### Challenge 0: **[Setup and prepare Environment](Student/Challenge-00.md)**

- Install the required development tools. This initial session is crucial to ensure that all participants are well-prepared and can fully engage with the workshop's content.

### Challenge 1: **[Accelerate your productivity with MCP servers in Visual Studio Code](Student/Challenge-01.md)**

- Learn how to boost your development productivity by integrating Model Context Protocol (MCP) servers directly into Visual Studio Code, enabling enhanced AI-powered development workflows.

### Challenge 2: **[Build your first MCP server](Student/Challenge-02.md)**

- Create your first Model Context Protocol server from scratch. Learn the fundamentals of MCP architecture and build a weather server that exposes tools and resources over standard transport protocols.kathon will provide a deep dive experience targeted for developers for building Agentic AI Applications. Hackathon is a collaborative learning experience, designed as a set of challenges to practice your technical skills. By participating in this hackathon, you will be able to understand the capabilities of Agentic AI Apps.

### Challenge 3: **[Build your first MCP client](Student/Challenge-03.md)**

- Develop an MCP client using C# and .NET that can connect to MCP servers, discover their capabilities, and interact with their tools through an AI assistant interface.

### Challenge 4: **[Host your MCP remote servers on ACA or Azure Functions](Student/Challenge-04.md)**

- Deploy your MCP server to the cloud using either Azure Container Apps (ACA) or Azure Functions, transforming local development tools into scalable, remotely accessible services.

### Challenge 5: **[Build your first AI Agent with AI Agents Service](Student/Challenge-05.md)**

- Create a sophisticated AI agent using Azure AI Agents Service with file search capabilities. Build a Travel Policy Compliance Agent that can analyze and answer questions using intelligent document retrieval.

### Challenge 6: **[Build your first Semantic Kernel App and integrate with MCP remote server](Student/Challenge-06.md)**

- Develop your first intelligent application using Semantic Kernel, Microsoft's lightweight SDK for AI agents. Create plugins for device control, time services, and integrate with your remote MCP weather server.

### Challenge 7: **[Tracing Intelligence: Observability in Agentic AI with Semantic Kernel](Student/Challenge-07.md)**

- Implement comprehensive observability for your AI applications using Azure AI Foundry Tracing UI or Aspire Dashboard. Learn to monitor, trace, and analyze the behavior of your intelligent services.

### Challenge 8: **[Develop Agentic AI Applications using Semantic Kernel and Multi-Agent Architectures](Student/Challenge-08.md)**

- Master advanced agentic AI development by creating multi-agent systems that collaborate to solve complex problems. Learn orchestration patterns including concurrent, sequential, handoff, and group chat approaches.

### Challenge 9: **[Secure your MCP remote server using an API key](Student/Challenge-09.md)**

- Enhance your MCP server security by implementing API key authentication. Learn to secure your remote MCP servers while enabling safe access from multiple clients over the internet.

### Challenge 10: **[Build Agentic RAG with Azure AI Search](Student/Challenge-10.md)**

- Create an advanced Agentic Retrieval-Augmented Generation system using Azure AI Search. Build intelligent agents that can dynamically decide what information to retrieve and how to synthesize comprehensive responses.

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
- [Microsoft Learn - Develop AI agents on Azure OpenAI and Semantic Kernel](https://learn.microsoft.com/en-us/training/paths/develop-ai-agents-azure-open-ai-semantic-kernel-sdk/)
- [Full Course (Lessons 1-11) MCP for Beginners](https://www.youtube.com/watch?v=VfZlglOWWZw)
- [Microsoft GitHub repo - MCP servers ](https://github.com/Microsoft/mcp)

## Repository Contents

- `./Student`
  - Student's Challenge Guide
- `./Student/Resources`
  - Resource files, sample code, scripts, etc meant to be provided to students. (Must be packaged up by the coach and provided to students at start of event)
- `./Coach`
  - Coach's Guide and related files
- `./Coach/Solutions`
  - Solution files with completed example answers to a challenge

## Remarks
- Please note that the content of this workshop may become outdated, as Azure AI is a rapidly evolving platform. We recommend staying engaged with the AI community for the most current updates and practices.
    
## Contributors
- Phanis Parpas
- Adrian Calinescu
