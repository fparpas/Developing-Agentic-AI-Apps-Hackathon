# Challenge 02 - C# - Build your first MCP server

 [< Previous Challenge](./Challenge-01.md) - **[Home](../README.md)** - [Next Challenge >](./Challenge-03-csharp.md)

[![](https://img.shields.io/badge/C%20Sharp-blue)](Challenge-02-csharp.md)
[![](https://img.shields.io/badge/Python-lightgray)](Challenge-02-python.md)

## Introduction

In this challenge, you will build and run a minimal Model Context Protocol (MCP) server locally, wire it into GitHub Copilot Chat in Visual Studio Code, and invoke the new tools and resources from chat.

## Concepts

The Model Context Protocol (MCP) is an open standard for connecting AI language models (LLMs) with external tools, resources, and prompts. It enables LLMs to extend their capabilities by calling out to specialized services, enhancing their functionality beyond text generation.

MCP servers provide tools, resources, and prompts over a standard transport. An IDE agent (like Copilot Chat) connects to your server, lists capabilities, and calls your tools with JSON inputs, receiving structured outputs.

- **Transport:** Most commonly, MCP servers run as local processes launched by VS Code or Copilot, communicating via stdio. Alternatively, MCP servers can be hosted remotely and accessed over a network, allowing multiple users or agents to connect.
- **Local vs Remote Servers:**
    - *Local servers* run on your machine, providing fast, direct integration with your IDE and access to local files or resources.
    - *Remote servers* are hosted elsewhere (e.g. in Azure Functions, Azure Container Apps), enabling centralized management, scalability, and access from different locations or users.
- **Capabilities:** Tools (functions you expose), resources (read-only data), prompts (templated guidance).
    - Tools:
        These are callable functions that your system makes available for use. They perform specific actions, such as fetching data, processing information, or interacting with external services. Think of them as the "active" capabilities your application provides.
    - Resources:
        These are datasets or information that users or systems can access but not modify. Resources might include documentation, configuration files, or reference tables. They provide valuable context or support without allowing changes.
    - Prompts:
        Prompts are pre-defined templates or instructions designed to guide users or systems in performing tasks. They help standardize interactions, ensuring consistency and clarity. For example, a prompt might be a template for asking a user to input specific information.

This architecture lets you choose between local development convenience and remote deployment flexibility, depending on your needs.

## Description

In this challenge, you will build a simple MCP weather server and connect it to a host (Visual Studio Code or Claude for Desktop).

Many LLMs do not currently have the ability to fetch real-time forecasts and severe weather alerts. Let’s use MCP to solve that.

You will build a server that exposes two tools: `get_alerts` and `get_forecast`, then connect the server to an MCP host (Visual Studio Code or Claude for Desktop).

> ℹ️ Servers can connect to any client. We’ve chosen Visual Studio Code or Claude Desktop here for simplicity, but you could also connect to other clients like Copilot Chat in JetBrains IDEs or even build your own client.

### Task 1: Set up your environment

Use the official MCP C# quickstart as your base: https://modelcontextprotocol.io/quickstart/server#c%23

1. Create a console app
```bash
dotnet new console -n WeatherMcpServer
cd WeatherMcpServer
```

2. Add the required NuGet packages for the Model Context Protocol SDK and hosting

```bash
# Add the Model Context Protocol SDK NuGet package
dotnet add package ModelContextProtocol --prerelease
# Add the .NET Hosting NuGet package
dotnet add package Microsoft.Extensions.Hosting
```

### Task 2: Scaffold your MCP server (C# / .NET)

Open the `Program.cs` file in your project and replace its contents with the following code. This sets up a basic console application that uses the Model Context Protocol SDK to create an MCP server with standard input/output (stdio) transport.
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;
using System.Net.Http.Headers;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});

var app = builder.Build();

await app.RunAsync();
```

### Task 3: Add weather tools to your MCP server

Add the following tools to your server:

- `get_forecast`: Returns a short forecast by latitude/longitude using api.weather.gov (NWS).
- `get_alerts`: Returns active severe weather alerts for a US state using api.weather.gov (NWS).

> ℹ️ These weather tools integrate with the National Weather Service API. It provides real-time weather updates and alerts for US locations only.

Create an extension class for HttpClient which helps simplify JSON request handling:
```csharp
using System.Text.Json;

internal static class HttpClientExt
{
    public static async Task<JsonDocument> ReadJsonDocumentAsync(this HttpClient client, string requestUri)
    {
        using var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    }
}
```

Next, define a class with the tool execution handlers for querying and converting responses from the National Weather Service API:
```csharp
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace QuickstartWeatherServer.Tools;

[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool, Description("Get weather alerts for a US state.")]
    public static async Task<string> GetAlerts(
        HttpClient client,
        [Description("The US state to get alerts for.")] string state)
    {
        using var jsonDocument = await client.ReadJsonDocumentAsync($"/alerts/active/area/{state}");
        var jsonElement = jsonDocument.RootElement;
        var alerts = jsonElement.GetProperty("features").EnumerateArray();

        if (!alerts.Any())
        {
            return "No active alerts for this state.";
        }

        return string.Join("\n--\n", alerts.Select(alert =>
        {
            JsonElement properties = alert.GetProperty("properties");
            return $"""
                    Event: {properties.GetProperty("event").GetString()}
                    Area: {properties.GetProperty("areaDesc").GetString()}
                    Severity: {properties.GetProperty("severity").GetString()}
                    Description: {properties.GetProperty("description").GetString()}
                    Instruction: {properties.GetProperty("instruction").GetString()}
                    """;
        }));
    }

    [McpServerTool, Description("Get weather forecast for a location.")]
    public static async Task<string> GetForecast(
        HttpClient client,
        [Description("Latitude of the location.")] double latitude,
        [Description("Longitude of the location.")] double longitude)
    {
        var pointUrl = string.Create(CultureInfo.InvariantCulture, $"/points/{latitude},{longitude}");
        using var jsonDocument = await client.ReadJsonDocumentAsync(pointUrl);
        var forecastUrl = jsonDocument.RootElement.GetProperty("properties").GetProperty("forecast").GetString()
            ?? throw new Exception($"No forecast URL provided by {client.BaseAddress}points/{latitude},{longitude}");

        using var forecastDocument = await client.ReadJsonDocumentAsync(forecastUrl);
        var periods = forecastDocument.RootElement.GetProperty("properties").GetProperty("periods").EnumerateArray();

        return string.Join("\n---\n", periods.Select(period => $"""
                {period.GetProperty("name").GetString()}
                Temperature: {period.GetProperty("temperature").GetInt32()}°F
                Wind: {period.GetProperty("windSpeed").GetString()} {period.GetProperty("windDirection").GetString()}
                Forecast: {period.GetProperty("detailedForecast").GetString()}
                """));
    }
}
```
### Task 4: Run and validate locally

From the `WeatherMcpServer` folder:
```bash
dotnet run
```
This starts the server and listens for incoming requests on standard input/output.

### Task 5: Connect to an MCP host

Option A: Visual Studio Code (GitHub Copilot Chat)
- Follow the VS Code MCP [guide:](https://code.visualstudio.com/docs/copilot/customization/mcp-servers#_use-mcp-tools-in-agent-mode)
- Add a server entry that invokes:
    - command: `dotnet`
    - args: `["run", "--project", "PATH_TO_YOUR_PROJECT"]`
    - transport: `stdio`
- Reload VS Code. In Copilot Chat, use `/tools` to see your server and try:
    - `get_forecast` with latitude/longitude
    - `get_alerts` with a two-letter state (e.g., WA)

Option B: Claude Desktop
- Follow the Claude MCP config [guide:](https://modelcontextprotocol.io/quickstart/server#testing-your-server-with-claude-for-desktop-5)

### Task 6: Use MCP Inspector for testing and debugging Model Context Protocol servers

The [MCP Inspector](https://modelcontextprotocol.io/legacy/tools/inspector) is an interactive developer tool for testing and debugging MCP servers. It lets you start/attach servers, call tools with JSON inputs, inspect requests/responses, and view logs.

1. Prerequisites
    - Node.js 18+ installed
    - Your `WeatherMcpServer` builds and runs locally

2. Start the Inspector
```bash
npx @modelcontextprotocol/inspector
```
The Inspector opens in your browser (or prints a local URL). Keep the terminal open.

3. Configure a server via the UI
     - In the Inspector, add the MCP server
     - Choose `stdio`
     - Command: `dotnet`
     - Args: `run --project "<ABSOLUTE_PATH>\WeatherMcpServer\WeatherMcpServer.csproj"`

     - Use the UI to:
         - List tools (`get_forecast`, `get_alerts`)
         - Invoke a tool and provide JSON input, for example:
             - `get_alerts`: `{ "state": "WA" }`
             - `get_forecast`: `{ "latitude": 47.6062, "longitude": -122.3321 }`

## Success Criteria

- ✅ A .NET MCP server runs locally over stdio.
- ✅ The server lists two tools: `get_forecast` and `get_alerts`.
- ✅ Invoking `get_forecast` returns current temperature (or basic forecast info) for the given latitude/longitude.
- ✅ Invoking `get_alerts` returns zero or more active alerts for the specified US state.
- ✅ Tools are visible and callable from your chosen MCP host (VS Code Copilot Chat or Claude Desktop).
- ✅ Validated with MCP Inspector: server connects via stdio, tools (`get_forecast`, `get_alerts`) invoke successfully, and requests/responses are visible without schema errors.
- ✅ The user gets a response to the prompt "Get the weather in Sacramento" when using the MCP tools in VS Code Copilot Chat or Claude Desktop.

## Learning Resources

- [Model Context Protocol (MCP) Overview](https://modelcontextprotocol.io/)
- [VS Code MCP Tools](https://code.visualstudio.com/docs/copilot/customization/mcp-servers#_use-mcp-tools-in-agent-mode)
- [MCP SDK Documentation](https://modelcontextprotocol.io/docs/sdk)
- [Quickstart (server)](https://modelcontextprotocol.io/quickstart/server#c%23)
- [VS Code MCP Servers](https://code.visualstudio.com/mcp)
- [GitHub Copilot in VS Code](https://code.visualstudio.com/docs/editor/github-copilot)
- [Weather.gov API](https://www.weather.gov/documentation/services-web-api)
- [MCP Inspector](https://modelcontextprotocol.io/legacy/tools/inspector)
- [Deep Dive: Understanding MCP Client-Server Communication with Agent and LLM](https://medium.com/@jamestang/deep-dive-understanding-mcp-client-server-communication-with-agent-and-llm-aa4782a65991)
