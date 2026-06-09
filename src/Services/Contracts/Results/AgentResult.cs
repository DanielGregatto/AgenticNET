using Domain.Contracts.Agent;
using System;
using System.Collections.Generic;

namespace Services.Contracts.Results
{
    /// <summary>Agent response returned by all chat endpoints.</summary>
    public record AgentResult(
        /// <summary>Name of the specialist agent that produced the answer.</summary>
        string AgentName,
        /// <summary>Conversation identifier. Echo this on subsequent requests to continue the conversation.</summary>
        string ConversationId,
        /// <summary>The agent's answer in plain text or Markdown.</summary>
        string Content,
        /// <summary>UTC timestamp of the response.</summary>
        DateTimeOffset Timestamp,
        /// <summary>
        /// Execution trace. Populated only when <c>includeTrace: true</c> is set in the request.
        /// Each step records a router decision, plugin call, plugin result, or reviewer score.
        /// </summary>
        IReadOnlyList<TraceStep>? Trace = null)
    {
        public static AgentResult FromDomain(AgentResponse response, bool includeTrace = false) =>
            new AgentResult(
                response.AgentName,
                response.ConversationId,
                response.Content,
                response.Timestamp,
                includeTrace ? response.Trace : null);
    }
}
