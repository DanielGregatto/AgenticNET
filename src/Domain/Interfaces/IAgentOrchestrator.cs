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

        /// <summary>
        /// Classifies the user message using the RouterAgent and returns the resolved specialist agent name.
        /// The system prompt is built dynamically from all registered non-router agents and their descriptions.
        /// Returns Failure if the router agent is misconfigured or the classifier returns an unrecognised agent name.
        /// </summary>
        Task<Result<string>> RouteAsync(
            string userMessage,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Classifies the user message via RouteAsync then delegates to the resolved specialist agent via SendMessageAsync.
        /// When canUseDefaultAgent is true and routing fails, falls back to the agent configured in DefaultAgentName.
        /// </summary>
        Task<Result<AgentResponse>> RouteAndSendAsync(
            string userMessage,
            string conversationId,
            bool canUseDefaultAgent = true,
            CancellationToken cancellationToken = default);

        /// <summary>Returns the list of agents declared in configuration.</summary>
        IReadOnlyList<AgentOptions> GetRegisteredAgents();
    }
}
