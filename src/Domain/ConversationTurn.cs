using Domain.Contracts.Agent;
using Domain.Core;
using System;
using System.Collections.Generic;

namespace Domain
{
    public class ConversationTurn : EntityBase<ConversationTurn>
    {
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        public string UserMessage { get; set; }
        public string AgentName { get; set; }
        public string AssistantMessage { get; set; }
        public List<TraceStep> TraceSteps { get; set; } = new();
    }
}
