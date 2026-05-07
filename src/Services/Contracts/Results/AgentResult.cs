using Domain.Contracts.Agent;
using System;

namespace Services.Contracts.Results
{
    public record AgentResult(
        string AgentName,
        string ConversationId,
        string Content,
        DateTimeOffset Timestamp)
    {
        public static AgentResult FromDomain(AgentResponse response) =>
            new AgentResult(
                response.AgentName,
                response.ConversationId,
                response.Content,
                response.Timestamp);
    }
}
