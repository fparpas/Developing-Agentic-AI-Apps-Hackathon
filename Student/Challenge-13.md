# Challenge 13 – Optional – Register and Discover Remote MCP Servers in Your API Inventory

[< Previous Challenge](./Challenge-12.md) - **[Home](../README.md)**

## Introduction

In this challenge, you'll learn how to use Azure API Center to maintain an inventory (registry) of remote Model Context Protocol (MCP) servers and enable stakeholders to discover them using the API Center portal. MCP servers expose backend APIs and data sources in a standardized way for consumption by AI agents and models.

Azure API Center provides a centralized platform for managing your organization's API ecosystem, including MCP servers that enable AI agents to access external data sources and services. By registering MCP servers in your API inventory, you make them discoverable to developers and other stakeholders, who can then integrate them into their agentic AI applications.

## Concepts

### Azure API Center

Azure API Center is a centralized platform for managing your organization's API ecosystem, including Model Context Protocol (MCP) servers. It provides a unified inventory and governance solution for all types of APIs across your organization, enabling better discovery, management, and governance of API assets.

#### Key features include:

- **API Inventory Management** – Maintain a comprehensive catalog of all APIs, including MCP servers, in your organization.
- **Discovery Portal** – Provide developers with a searchable interface to find and explore APIs.
- **Environment Management** – Configure different environments (development, staging, production) for your APIs.
- **Governance and Compliance** – Apply consistent policies and standards across your API portfolio.
- **Integration Capabilities** – Automatically synchronize with Azure API Management and other API platforms.

#### API Center Components

- **APIs** – Core API definitions that represent your API assets, including MCP servers.
- **Versions** – Different versions of your APIs, each with its own specifications and metadata.
- **Environments** – Logical groupings that represent different deployment stages (dev, test, prod).
- **Deployments** – Specific instances of API versions running in particular environments.
- **API Definitions** – Technical specifications (OpenAPI, AsyncAPI, etc.) that describe your APIs.

#### MCP Server Management in API Center

For Model Context Protocol servers specifically, Azure API Center enables you to:

- Maintain a centralized inventory of MCP servers alongside traditional APIs.
- Register both custom and partner MCP servers with the "MCP" API type.
- Provide discovery capabilities for AI developers through the API Center portal.
- Configure environments, deployments, and definitions for MCP servers.
- Leverage the same governance and management capabilities as traditional APIs.

## Description

Your task is to set up an Azure API Center instance and register MCP servers in your API inventory to make them discoverable to your organization's developers and AI application builders.

### Task 1: Set up Azure API Center

1. Create an Azure API Center instance in your subscription.
2. Configure the API Center with appropriate naming and a resource group.
3. Set up the API Center portal for developer discovery.

### Task 2: Integration with API Management

In your Azure API Management instance:

1. Enable automatic synchronization between API Management and API Center.
2. Verify that MCP servers from API Management are automatically imported.
3. Test the synchronization process.

### Task 3: Manually Register a Custom MCP Server

1. Register a custom MCP server in your API inventory.
2. Set the API type to "MCP".
3. Configure an environment for your MCP server (e.g., "Production", "Development").
4. Create a deployment with the runtime URL for your MCP service.

### Task 4: Register Partner MCP Servers

1. Browse the curated list of partner MCP servers in Azure API Center.
2. Register one or more partner MCP servers from Microsoft services (such as Azure Logic Apps, GitHub, etc.).
3. Verify that the partner servers are automatically configured with:
   - API entry with MCP type.
   - Environment and deployment.
   - OpenAPI definition (if available).

### Task 5: Configure API Center Portal

1. Set up the API Center portal for your organization.
2. Configure user access and permissions.
3. Test the discovery experience by browsing registered MCP servers.
4. Verify that developers can view MCP server details, including URL endpoints.


## Success Criteria

- ✅ An Azure API Center instance is successfully created and configured with portal access.
- ✅ Appropriate environments, deployments, and access controls are set up.
- ✅ At least one custom MCP server is registered with API type "MCP".
- ✅ At least one partner MCP server is registered from the curated list (Microsoft Learn, GitHub, etc.).
- ✅ The API Center portal is accessible and functional for developers.
- ✅ MCP servers are discoverable in the portal with proper filtering and browsing capabilities.
- ✅ MCP servers have appropriate descriptions and metadata for governance.
- ✅ API Management synchronization is working with automated import of MCP servers.

## Learning Resources

- [Azure API Center Overview](https://learn.microsoft.com/en-us/azure/api-center/overview)
- [Register and discover remote MCP servers in your API inventory](https://learn.microsoft.com/en-us/azure/api-center/register-discover-mcp-server)
- [Tutorial: Register APIs in your API inventory](https://learn.microsoft.com/en-us/azure/api-center/tutorials/register-apis)
- [Tutorial: Add environments and deployments for APIs](https://learn.microsoft.com/en-us/azure/api-center/tutorials/configure-environments-deployments)
- [Set up API Center portal](https://learn.microsoft.com/en-us/azure/api-center/set-up-api-center-portal)
- [Synchronize APIs from Azure API Management instance](https://learn.microsoft.com/en-us/azure/api-center/synchronize-api-management-apis)

