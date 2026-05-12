using Domain.Contracts.Common;
using MediatR;
using Services.Contracts.Results;

namespace Services.Features.Agent.Commands.Chat
{
    public class ChatCommand : IRequest<Result<AgentResult>>
    {
        public string Message { get; set; }

        /// <summary>
        /// Client-supplied conversation identifier for history continuity.
        /// When null or empty the handler generates a new conversation ID.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// When true and the router cannot resolve a specialist agent, the request falls back to the DefaultAgent.
        /// Defaults to true.
        /// </summary>
        public bool CanUseDefaultAgent { get; set; } = true;
    }
}
