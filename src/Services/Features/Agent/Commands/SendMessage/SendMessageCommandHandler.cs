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

namespace Services.Features.Agent.Commands.SendMessage
{
    public class SendMessageCommandHandler : BaseCommandHandler,
        IRequestHandler<SendMessageCommand, Result<AgentResult>>
    {
        private readonly IAgentOrchestrator _orchestrator;
        private readonly IValidator<SendMessageCommand> _validator;
        private readonly ILogger<SendMessageCommandHandler> _logger;

        public SendMessageCommandHandler(
            AppDbContext context,
            IUser user,
            IAgentOrchestrator orchestrator,
            IValidator<SendMessageCommand> validator,
            ILogger<SendMessageCommandHandler> logger)
            : base(context, user)
        {
            _orchestrator = orchestrator;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<AgentResult>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending message to agent {AgentName}", request.AgentName);

            var validationError = await ValidateAsync<SendMessageCommand, AgentResult>(_validator, request, cancellationToken);
            if (validationError != null)
                return validationError;

            if (string.IsNullOrWhiteSpace(request.ConversationId))
                request.ConversationId = Guid.NewGuid().ToString();

            var result = await _orchestrator.SendMessageAsync(
                request.AgentName,
                request.Message,
                request.ConversationId,
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning("Agent '{AgentName}' returned failure: {Errors}",
                    request.AgentName, string.Join(", ", result.Errors.Select(e => e.Message)));

                return Result<AgentResult>.Failure(result.Errors.ToArray());
            }

            _logger.LogInformation("Agent {AgentName} responded on conversation {ConversationId}",
                request.AgentName, request.ConversationId);

            return Result<AgentResult>.Success(AgentResult.FromDomain(result.Data, request.IncludeTrace));
        }
    }
}
