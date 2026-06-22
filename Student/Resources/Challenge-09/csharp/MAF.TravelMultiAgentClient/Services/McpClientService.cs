using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace TravelMultiAgentClient.Services;

public class McpClientService
{
    private McpClient? _mcpClient;
    private string _baseUrl;

    public McpClientService(IConfiguration configuration)
    {
        var baseUrl = configuration["TravelMcpServer:BaseUrl"] ?? "http://localhost:3000";
        _baseUrl = baseUrl;

        // Note: Constructors cannot be async, so initialize _mcpClient synchronously or move async initialization elsewhere.
        // If async initialization is required, consider using a separate async Init method or a factory pattern.
                _mcpClient = McpClient.CreateAsync(
           new HttpClientTransport(
               new HttpClientTransportOptions()
               {
                   Endpoint = new Uri(_baseUrl)
               }
           )
        ).GetAwaiter().GetResult();
    }

    public IList<McpClientTool> McpTools {
        get { return GetMcpTools(); }
    }

    public IList<McpClientTool> GetMcpTools()
    {
        var tools = _mcpClient!.ListToolsAsync().GetAwaiter().GetResult();
        return tools;
    }

        public async Task<IList<McpClientTool>> GetMcpToolsAsync()
    {
        var tools = await _mcpClient!.ListToolsAsync();
        return tools;
    }
}