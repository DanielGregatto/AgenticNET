using System;

namespace Domain.Contracts.Agent
{
    public record AgentResponse(
        string AgentName,
        string ConversationId,
        string Content,
        DateTimeOffset Timestamp);
}
