using Domain.Contracts.Common;
using MediatR;
using Services.Contracts.Results;
using System;

namespace Services.Features.Conversation.Commands.GetOrCreate
{
    public class GetOrCreateConversationCommand : IRequest<Result<ConversationResult>>
    {
        public Guid ConversationId { get; set; }
    }
}
