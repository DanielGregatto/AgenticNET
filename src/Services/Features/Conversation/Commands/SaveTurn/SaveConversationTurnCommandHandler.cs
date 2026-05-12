using Data.Context;
using Domain;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Conversation.Commands.SaveTurn
{
    public class SaveConversationTurnCommandHandler : BaseCommandHandler,
        IRequestHandler<SaveConversationTurnCommand, Result>
    {
        private readonly ILogger<SaveConversationTurnCommandHandler> _logger;

        public SaveConversationTurnCommandHandler(
            AppDbContext context,
            IUser user,
            ILogger<SaveConversationTurnCommandHandler> logger)
            : base(context, user)
        {
            _logger = logger;
        }

        public async Task<Result> Handle(
            SaveConversationTurnCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _context.ConversationMessages.Add(new ConversationMessage
                {
                    ConversationId = request.ConversationId,
                    Role = "User",
                    Content = request.UserMessage
                });

                _context.ConversationMessages.Add(new ConversationMessage
                {
                    ConversationId = request.ConversationId,
                    Role = "Assistant",
                    Content = request.AssistantMessage
                });

                _context.ConversationTurns.Add(new ConversationTurn
                {
                    ConversationId = request.ConversationId,
                    UserMessage = request.UserMessage,
                    AgentName = request.AgentName,
                    AssistantMessage = request.AssistantMessage,
                    TraceSteps = request.TraceSteps?.ToList() ?? new()
                });

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Saved turn for conversation {ConversationId}, agent {AgentName}",
                    request.ConversationId, request.AgentName);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save turn for conversation {ConversationId}", request.ConversationId);

                return Result.Failure(
                    $"Failed to persist conversation turn: {ex.Message}", ErrorTypes.External);
            }
        }
    }
}
