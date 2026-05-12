using Data.Context;
using Domain;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Contracts.Results;
using Services.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Conversation.Commands.GetOrCreate
{
    public class GetOrCreateConversationCommandHandler : BaseCommandHandler,
        IRequestHandler<GetOrCreateConversationCommand, Result<ConversationResult>>
    {
        private readonly ILogger<GetOrCreateConversationCommandHandler> _logger;

        public GetOrCreateConversationCommandHandler(
            AppDbContext context,
            IUser user,
            ILogger<GetOrCreateConversationCommandHandler> logger)
            : base(context, user)
        {
            _logger = logger;
        }

        public async Task<Result<ConversationResult>> Handle(
            GetOrCreateConversationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

                if (conversation is null)
                {
                    var userId = UserID.ToString();

                    conversation = new Domain.Conversation
                    {
                        Id = request.ConversationId,
                        UserId = userId
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Created conversation {ConversationId} for user {UserId}",
                        request.ConversationId, userId);

                    return Result<ConversationResult>.Success(
                        new ConversationResult(conversation.Id, userId, new()));
                }

                var messages = conversation.Messages
                    .Select(m => new ConversationMessageResult(m.Role, m.Content))
                    .ToList();

                return Result<ConversationResult>.Success(
                    new ConversationResult(conversation.Id, conversation.UserId, messages));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get or create conversation {ConversationId}", request.ConversationId);

                return Result<ConversationResult>.Failure(
                    $"Failed to load conversation history: {ex.Message}", ErrorTypes.External);
            }
        }
    }
}
