using System;
using System.Collections.Generic;

namespace Domain.Contracts.Agent
{
    public record AgentResponse(
        string AgentName,
        string ConversationId,
        string Content,
        DateTimeOffset Timestamp,
        IReadOnlyList<TraceStep>? Trace = null);
}
