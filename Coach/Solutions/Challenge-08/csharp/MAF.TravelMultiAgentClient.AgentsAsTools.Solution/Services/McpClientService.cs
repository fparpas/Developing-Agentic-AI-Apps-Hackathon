using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI.Responses;
using System.Text;
using System.Text.Json;

namespace TravelMultiAgentClient.Services;

public class McpClientService
{
    private IMcpClient _mcpClient;
    private ILogger<McpClientService> _logger;
    private string _baseUrl;

    public McpClientService(IConfiguration configuration, ILogger<McpClientService> logger)
    {
        var baseUrl = configuration["TravelMcpServer:BaseUrl"] ?? "http://localhost:3000";
        _baseUrl = baseUrl;
        _logger = logger;

        // Note: Constructors cannot be async, so initialize _mcpClient synchronously or move async initialization elsewhere.
        // If async initialization is required, consider using a separate async Init method or a factory pattern.
        _mcpClient = McpClientFactory.CreateAsync(
            new SseClientTransport(
                new SseClientTransportOptions
                {
                    Endpoint = new Uri(baseUrl),
                    ConnectionTimeout = TimeSpan.FromMinutes(5), // Increase MCP connection timeout to 5 minutes
                }
            )
        ).GetAwaiter().GetResult();
    }

    public IList<McpClientTool> McpTools {
        get { return GetMcpTools(); }
    }

    public IList<McpClientTool> GetMcpTools()
    {
        var tools = _mcpClient.ListToolsAsync().GetAwaiter().GetResult();
        return tools;
    }

        public async Task<IList<McpClientTool>> GetMcpToolsAsync()
    {
        var tools = await _mcpClient.ListToolsAsync();
        return tools;
    }
}