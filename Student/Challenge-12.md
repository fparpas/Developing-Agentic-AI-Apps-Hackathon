# Challenge 12 - Optional - Expose REST API in API Management as an MCP server

[< Previous Challenge](./Challenge-11.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-13.md)

## Introduction

In the previous challenge, you secured access to existing MCP servers using Azure API Management. You can also transform any REST API managed in Azure API Management into an MCP server.

This challenge covers the second major MCP use case in Azure API Management: automatically exposing REST APIs as MCP servers so AI agents can use them as tools without custom MCP code. API operations automatically become MCP tools that agents can discover and invoke.

You will work with the **National Weather Service API** (`api.weather.gov`), a public REST API that provides forecasts, alerts, and observational data across the United States. It illustrates how an existing API can become a powerful agent tool without additional server implementation.

## Concepts

### Transforming a REST API into an MCP Server

Azure API Management can automatically transform any REST API into an MCP (Model Context Protocol) server, enabling AI agents to use existing APIs as tools without custom code. The transformation leverages OpenAPI specifications to generate MCP tools that map directly to REST API operations.

### OpenAPI Specification Import

OpenAPI specifications provide machine‑readable descriptions of REST APIs, including endpoints, parameters, request/response schemas, and authentication methods. API Management can import these specifications to create fully managed API proxies with automatic documentation, validation, and governance.

### MCP Tool Generation

When exposing a REST API as an MCP server, each API operation automatically becomes an MCP tool:

- **Tool Names**: Generated from operation IDs or endpoint paths
- **Parameters**: Mapped from OpenAPI parameter definitions
- **Schemas**: Request and response schemas are preserved for validation
- **Documentation**: API descriptions become tool descriptions for AI agents

### Agent Tool Discovery

AI agents discover available weather tools through the MCP protocol:

- **Tool listing**: Query available tools and their capabilities.
- **Parameter schemas**: Understand required and optional parameters.
- **Response formats**: Know the data shape returned by each tool.
- **Tool chaining**: Combine multiple tools to answer multi‑step queries.

### API Management Integration Benefits

Exposing REST APIs through API Management provides enterprise‑grade capabilities:

- **Security**: Authentication, authorization, and API key management.
- **Governance**: Rate limiting, quotas, and usage policies.
- **Monitoring**: Analytics, logging, and performance metrics.
- **Reliability**: Caching, retry logic, and circuit breaker patterns.
- **Documentation**: Automatic API documentation and developer portal integration.

## Description

In this challenge, you will import the National Weather Service API into Azure API Management using its OpenAPI specification, then expose that REST API as an MCP server. This enables AI agents to access real‑time weather data through standardized MCP tools.

The National Weather Service offers a free API with current conditions, forecasts, alerts, and station observation data for the United States.

### Task 1: Import National Weather Service API into API Management

Import the weather API using its OpenAPI specification to create a managed API in Azure API Management.

1. **Access Your API Management Instance**:
   - Navigate to your Azure API Management instance from previous challenges
   - If you don't have one, follow the [API Management quickstart](https://learn.microsoft.com/en-us/azure/api-management/get-started-create-service-instance)

2. **Import the OpenAPI Specification**:
   - Import the API using the OpenAPI specification from: `https://api.weather.gov/openapi.json`
   - Configure with display name "National Weather Service API" and URL suffix "weather"

3. **Verify API Import**:
   - Confirm operations import successfully, including endpoints for coordinates, forecasts, alerts, and observations.
   - Review automatically generated documentation for completeness and accuracy.

4. **Test the API**:
   - Use the **Test** tab to verify connectivity.
   - Send a sample request.
   - Confirm valid responses are returned from the National Weather Service.

### Task 2: Create MCP Server from REST API

Transform your imported REST API into an MCP server that AI agents can discover and use.

1. **Create MCP Server from API**:
   - Navigate to **MCP servers** in your API Management instance
   - Create a new MCP server by exposing the National Weather Service API as an MCP server
   - Configure the server with name "Weather Tools Server", base path "weather-tools", and appropriate description

2. **Review Generated MCP Tools**:
   - Verify that API operations convert to MCP tools with descriptive names.
   - Confirm parameter schemas are mapped from the OpenAPI specification.
   - Note key tools such as `get_points_by_coordinates`, `get_gridpoint_forecast`, `get_alerts`, and `get_station_observations`.

3. **Configure Tool Documentation**:
   - Review and enhance tool descriptions for AI agent consumption
   - Ensure parameter descriptions are clear and response schemas are properly documented

### Task 3: Test MCP Server with MCP Inspector

Verify that your weather MCP server works correctly with MCP clients.

1. **Connect MCP Inspector**:
   - Launch MCP Inspector and connect to your MCP server URL: `https://<your-apim-name>.azure-api.net/weather-tools/mcp`

2. **Validate Weather Tools**:
   - Explore available tools and verify documentation quality.
   - Test key operations: location metadata, forecasts, current conditions, and alerts.
   - Confirm responses contain valid, current weather data with accurate timestamps and location information.

### Optional Task 4: Integrate with AI Agents

Create an AI agent that can use your weather MCP server to answer weather-related questions and provide intelligent recommendations.

## Success Criteria

- ✅ National Weather Service API is successfully imported into API Management using OpenAPI specification
- ✅ REST API operations are properly imported with correct parameters and documentation
- ✅ MCP server is created from the imported REST API with all tools properly configured
- ✅ Weather tools are available and functional through MCP Inspector
- ✅ Tools can retrieve real-time weather data including forecasts, current conditions, and alerts
- ✅ MCP server correctly handles location-based queries with coordinate parameters
- ✅ AI agents can successfully discover, call, and receive valid responses from weather tools


## Learning Resources

### Azure API Management
- [Quickstart: Create a new Azure API Management instance by using the Azure portal](https://learn.microsoft.com/en-us/azure/api-management/get-started-create-service-instance)
- [Import an OpenAPI specification | Microsoft Learn](https://learn.microsoft.com/en-us/azure/api-management/import-api-from-oas?tabs=portal)
- [Expose REST API in API Management as an MCP server | Microsoft Learn](https://learn.microsoft.com/en-us/azure/api-management/export-rest-mcp-server)
- [About MCP servers in Azure API Management](https://learn.microsoft.com/en-us/azure/api-management/mcp-server-overview)

### National Weather Service API
- [National Weather Service API Documentation](https://www.weather.gov/documentation/services-web-api)
