using Domain.Configs;
using Domain.Contracts.Agent;
using Domain.Contracts.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAgentOrchestrator
    {
        /// <summary>
        /// Sends a user message to the named agent and returns its response.
        /// Conversation history is maintained per conversationId.
        /// Returns NotFound if the agent name is not registered, Failure if the provider is misconfigured.
        /// </summary>
        Task<Result<AgentResponse>> SendMessageAsync(
            string agentName,
            string userMessage,
            string conversationId,
            CancellationToken cancellationToken = default);

        /// <summary>Returns the list of agents declared in configuration.</summary>
        IReadOnlyList<AgentOptions> GetRegisteredAgents();
    }
}
