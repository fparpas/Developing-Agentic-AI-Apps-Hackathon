using Azure.AI.Projects;
using Microsoft.Agents.AI;

namespace TravelMultiAgentClient.Agents;

public class TravelPolicyAgent
{
    private readonly AIAgent _agent;

    public AIAgent Agent
    {
        get { return _agent; }
    }

    public TravelPolicyAgent(AIProjectClient projectClient, string agentName)
    {
        // Retrieve the Foundry Agent Service agent as an AIAgent, matching the
        // Challenge-06 pattern (AIProjectClient.GetAIAgentAsync).
        _agent = projectClient.GetAIAgentAsync(agentName).Result;
    }
}