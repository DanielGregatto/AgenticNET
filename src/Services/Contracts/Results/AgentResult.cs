using Domain.Contracts.Agent;
using System;
using System.Collections.Generic;

namespace Services.Contracts.Results
{
    public record AgentResult(
        string AgentName,
        string ConversationId,
        string Content,
        DateTimeOffset Timestamp,
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
