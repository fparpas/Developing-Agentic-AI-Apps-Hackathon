![](https://img.shields.io/badge/For%20Final%20Review-orange)
![](https://img.shields.io/badge/Collect%20Feedback-orange)

# Challenge 04 - C# - Host MCP Remote Servers on ACA or Azure Functions

 [< Previous Challenge](./Challenge-03-csharp.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-05-csharp.md)
 
[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-04-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-04-python.md)


## Introduction

In this challenge, you'll build and deploy a remote Model Context Protocol (MCP) server that can be accessed over HTTP. Unlike the previous challenges where the MCP server ran locally with stdio transport, this challenge focuses on creating a web-accessible MCP server that can be consumed by remote clients.

You'll start with an incomplete WeatherRemoteMcpServer project, complete the implementation, and deploy it to either Azure Container Apps (ACA) or Azure Functions. Both deployment options are provided, giving you flexibility to choose the Azure service that best fits your needs.

## Key Concepts

Understanding these core concepts will help you succeed in this challenge.

### MCP Transport: From Local to Remote

**Previous Challenges (stdio):**
- MCP server and client ran on the same machine
- Direct communication through input/output streams
- Perfect for local development and testing

**This Challenge (HTTP):**
- MCP server runs in the cloud, accessible from anywhere
- Clients connect over the internet using Http web protocols
- Enables multiple clients to use the same server simultaneously

### Azure Deployment: Two Simple Options

**Azure Container Apps**
- Best for: Apps that need flexibility and control
- Scaling: Automatically adjusts to traffic, scales to zero when idle
- Cost: Pay only for what you use
- Think of it as: A smart hosting service for containerized apps

**Azure Functions**
- Best for: Simple APIs with minimal management
- Scaling: Handles everything automatically
- Cost: Extremely low cost for light usage
- Think of it as: Run your code only when needed

**How to Choose:**
- **Container Apps**: If you want more control and expect regular traffic
- **Functions**: If you want maximum simplicity and minimal cost

## Description

In this challenge, you'll complete and deploy a **WeatherRemoteMcpServer** that implements the Model Context Protocol over HTTP transport, provides weather forecasting and alert tools, runs as a containerized web application, can be accessed remotely by MCP clients, and integrates with the National Weather Service API.

You'll build a weather MCP server that runs in Azure instead of locally, can be accessed by remote clients from anywhere, provides real-time weather forecasts and alerts, and scales automatically based on usage demand.

Moving from local to remote MCP servers unlocks significant advantages: multiple AI agents can use your server simultaneously, the service is always available without requiring local execution, it handles many requests automatically without your intervention, and follows production-ready deployment patterns used in real-world applications.

> **📝 Note:** For simplicity, this challenge does not implement authentication or authorization. The MCP server will be publicly accessible without security restrictions. Authentication and authorization patterns for production MCP servers will be covered in upcoming challenges.

This challenge consists of three main tasks that build upon each other:

#### Task 1: Complete the MCP Server Implementation
**Goal:** Get the WeatherRemoteMcpServer running with HTTP transport

**What you'll do:**
- Complete the incomplete `Program.cs` file
- Configure MCP server with HTTP transport instead of stdio
- Add necessary ASP.NET Core services and MCP endpoints
- Ensure the application runs on port 8080 and handles MCP protocol requests

##### Project Structure

Your project starting point is located at [Resources/Challenge-04/src/WeatherRemoteMcpServer/](./Resources/Challenge-04/src/WeatherRemoteMcpServer/)


```
📁 Resources/Challenge-04/src/WeatherRemoteMcpServer/
├── 📄 WeatherRemoteMcpServer.csproj    # Project file with MCP dependencies
├── 📄 Program.cs                       # ⚠️  INCOMPLETE - You need to complete this
├── 📄 HttpClientExt.cs                 # HTTP client extensions (provided)
├── 📄 Dockerfile                       # Container Apps Dockerfile (provided)
├── 📄 Dockerfile.functions             # Azure Functions Dockerfile (provided)
└── 📂 Tools/
    └── 📄 WeatherTools.cs              # Weather API tools (provided)
```

#### Task 2: Deploy to Azure
**Goal:** Get your MCP server running in the cloud using predefined deployment scripts

**Description:**
Once your MCP server is working locally, it's time to deploy it to Azure so it can be accessed remotely over the internet. We've provided complete automation scripts for both Azure Container Apps and Azure Functions, along with detailed README files that walk you through the entire deployment process step-by-step.

**Choose your deployment method:**
- **Option A:** Azure Container Apps (recommended for flexibility)
  - Script: [deploy-aca-script.ps1](./Resources/Challenge-04/deploy-aca-script.ps1)
  - Instructions: [Azure Container Apps README](./Resources/Challenge-04/README-ACA.md)
- **Option B:** Azure Functions (recommended for simplicity)
  - Script: [deploy-functions-script.ps1](./Resources/Challenge-04/deploy-functions-script.ps1)
  - Instructions: [Azure Functions README](./Resources/Challenge-04/README-Functions.md)

**What you'll do:**
- Choose your preferred deployment option and read the corresponding README file
- Use the provided predefined deployment script for your chosen option
- Run the script to automatically create and deploy all Azure resources
- Verify your deployment is accessible via the provided URL

#### Task 3: Test with MCP Inspector
**Goal:** Verify your remote MCP server works with real MCP clients

**What you'll do:**
- Run the official MCP Inspector tool
- Connect to your deployed Azure server using HTTP transport
- Test the available weather tools with real data
- Demonstrate successful remote tool execution

## Success Criteria

- ✅ Complete the MCP server implementation with HTTP transport
- ✅ Application builds and runs without errors on port 8080
- ✅ Server responds with appropriate MCP protocol messages when accessed
- ✅ Successfully deploy to either Azure Container Apps or Azure Functions
- ✅ Application is accessible via public Azure URL
- ✅ Basic connectivity test confirms MCP server is running in the cloud
- ✅ MCP Inspector successfully connects to your deployed server
- ✅ Weather tools (`get_forecast` and `get_alerts`) are visible and functional
- ✅ Tools work with real data from the National Weather Service
- ✅ Complete end-to-end remote MCP server functionality demonstration


## Learning Resources
### Deployment Documentation
- 📖 [Azure Container Apps README](./Resources/Challenge-04/README-ACA.md)
- 📖 [Azure Functions README](./Resources/Challenge-04/README-Functions.md)

### MCP Documentation
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP .NET SDK Documentation](https://github.com/microsoft/mcp-dotnet)

### Azure Services
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)