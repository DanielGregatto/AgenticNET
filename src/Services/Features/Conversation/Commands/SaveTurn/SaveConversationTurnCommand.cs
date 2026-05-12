using Domain.Contracts.Agent;
using Domain.Contracts.Common;
using MediatR;
using System;
using System.Collections.Generic;

namespace Services.Features.Conversation.Commands.SaveTurn
{
    public class SaveConversationTurnCommand : IRequest<Result>
    {
        public Guid ConversationId { get; set; }
        public string UserMessage { get; set; }
        public string AgentName { get; set; }
        public string AssistantMessage { get; set; }
        public IReadOnlyList<TraceStep> TraceSteps { get; set; }
    }
}
