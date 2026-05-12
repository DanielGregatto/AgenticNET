using Data.Context;
using Domain.Contracts.Common;
using Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Services.Contracts.Results;
using Services.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Features.Agent.Commands.Chat
{
    public class ChatCommandHandler : BaseCommandHandler,
        IRequestHandler<ChatCommand, Result<AgentResult>>
    {
        private readonly IAgentOrchestrator _orchestrator;
        private readonly IValidator<ChatCommand> _validator;
        private readonly ILogger<ChatCommandHandler> _logger;

        public ChatCommandHandler(
            AppDbContext context,
            IUser user,
            IAgentOrchestrator orchestrator,
            IValidator<ChatCommand> validator,
            ILogger<ChatCommandHandler> logger)
            : base(context, user)
        {
            _orchestrator = orchestrator;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<AgentResult>> Handle(ChatCommand request, CancellationToken cancellationToken)
        {
            var validationError = await ValidateAsync<ChatCommand, AgentResult>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            if (string.IsNullOrWhiteSpace(request.ConversationId))
                request.ConversationId = Guid.NewGuid().ToString();

            var result = await _orchestrator.RouteAndSendAsync(
                request.Message,
                request.ConversationId,
                request.CanUseDefaultAgent,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning("Chat routing failed: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Message)));

                return Result<AgentResult>.Failure(result.Errors.ToArray());
            }

            _logger.LogInformation(
                "Chat routed to agent '{AgentName}' on conversation '{ConversationId}'",
                result.Data.AgentName, request.ConversationId);

            return Result<AgentResult>.Success(AgentResult.FromDomain(result.Data));
        }
    }
}
