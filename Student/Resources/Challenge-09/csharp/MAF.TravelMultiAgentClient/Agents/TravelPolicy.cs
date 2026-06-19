using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using System.ComponentModel;
using TravelMultiAgentClient.Services;

namespace TravelMultiAgentClient.Agents;

public class TravelPolicyAgent
{
    private readonly McpClientService _mcpClient;
    private readonly ILogger<TravelPolicyAgent> _logger;
    private readonly AIAgent _agent;

    const string SourceName = "WorkflowSample";
    public AIAgent Agent
    {
        get { return _agent; }
    }

    public TravelPolicyAgent(PersistentAgentsClient persistentAgentsClient, string agentId)
    {
        _agent = persistentAgentsClient.GetAIAgentAsync(agentId).Result;
    }
}