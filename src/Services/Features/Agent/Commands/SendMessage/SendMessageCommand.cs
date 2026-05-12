using Domain.Contracts.Common;
using MediatR;
using Services.Contracts.Results;

namespace Services.Features.Agent.Commands.SendMessage
{
    public class SendMessageCommand : IRequest<Result<AgentResult>>
    {
        public string AgentName { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Client-supplied conversation identifier for history continuity.
        /// When null or empty the handler generates a new conversation ID.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// When true the response includes the full execution trace (plugin calls, reviewer result).
        /// Defaults to false.
        /// </summary>
        public bool IncludeTrace { get; set; } = false;
    }
}
