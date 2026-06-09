using Domain.Contracts.API;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts.Results;
using Services.Features.Agent.Commands.Chat;
using Services.Features.Agent.Commands.SendMessage;
using Services.Features.Agent.Queries.ListAgents;
using UI.API.Controllers.Base;

namespace UI.API.Controllers
{
    /// <summary>
    /// Core AI endpoints. Route a message to the best specialist agent, address a named agent directly,
    /// or list the agents registered in configuration. All endpoints require a Bearer JWT.
    /// </summary>
    public class AgentController : CoreController
    {
        private readonly IMediatorHandler _mediator;
        private readonly ILogger<AgentController> _logger;

        public AgentController(IMediatorHandler mediator, ILogger<AgentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Sends a message to the router, which classifies the intent and delegates to the most appropriate specialist agent.
        /// Supply the same ConversationId across requests to continue a conversation.
        /// Omit ConversationId to start a new conversation (a new ID is generated and returned).
        /// Set CanUseDefaultAgent to false to receive an error instead of a fallback when routing fails.
        /// </summary>
        [HttpPost("v1/chat")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<AgentResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        public async Task<IActionResult> Chat([FromBody] ChatCommand command)
        {
            _logger.LogInformation(
                "Chat request received, conversation {ConversationId}",
                command.ConversationId);

            var result = await _mediator.SendCommand(command);
            return Response(result);
        }

        /// <summary>
        /// Returns the list of agents registered in configuration.
        /// </summary>
        [HttpGet("v1/agents")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<AgentListItemResult>>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        public async Task<IActionResult> ListAgents()
        {
            _logger.LogInformation("Listing registered agents");

            var result = await _mediator.SendCommand(new ListAgentsQuery());
            return Response(result);
        }

        /// <summary>
        /// Sends a message to the specified agent and returns its response.
        /// Supply the same ConversationId across requests to continue a conversation.
        /// Omit ConversationId to start a new conversation (a new ID is generated and returned).
        /// </summary>
        /// <param name="agentName">The name of the agent to address, as declared in configuration.</param>
        /// <param name="command">Message payload.</param>
        [HttpPost("v1/agents/{agentName}/messages")]
        [Authorize]
        [ProducesResponseType(typeof(SuccessResponse<AgentResult>), 200)]
        [ProducesResponseType(typeof(ErrorResponseDto), 400)]
        [ProducesResponseType(typeof(ErrorResponseDto), 401)]
        [ProducesResponseType(typeof(ErrorResponseDto), 404)]
        public async Task<IActionResult> SendMessage(
            [FromRoute] string agentName,
            [FromBody] SendMessageCommand command)
        {
            command.AgentName = agentName;

            _logger.LogInformation(
                "Received message for agent {AgentName}, conversation {ConversationId}",
                agentName, command.ConversationId);

            var result = await _mediator.SendCommand(command);

            if (result.IsSuccess)
                _logger.LogInformation(
                    "Agent {AgentName} responded on conversation {ConversationId}",
                    agentName, result.Data?.ConversationId);
            else
                _logger.LogWarning(
                    "Agent {AgentName} failed: {Errors}",
                    agentName, string.Join(", ", result.Errors.Select(e => e.Message)));

            return Response(result);
        }
    }
}
