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
        /// Send a message and let the router pick the best agent.
        /// </summary>
        /// <remarks>
        /// The RouterAgent classifies the message intent and dispatches to the matching specialist
        /// (GeneralAdvisor, ProductCatalog, SupplierAdvisor, ...).
        ///
        /// - Omit `conversationId` to start a new conversation; the generated ID is returned and should
        ///   be echoed on subsequent requests to maintain history.
        /// - Set `canUseDefaultAgent: false` to receive a 400 error instead of falling back to the
        ///   default agent when no specialist matches.
        /// - Set `includeTrace: true` to receive the full decision trail (router choice, plugin calls,
        ///   reviewer score) in the `trace` array of the response.
        /// </remarks>
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
        /// List all agents registered in configuration.
        /// </summary>
        /// <remarks>
        /// Returns name, description, provider, model, and plugins for each agent.
        /// Useful for discovering which agents are available before calling
        /// <c>POST /api/v1/agents/{agentName}/messages</c>.
        /// </remarks>
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
        /// Send a message directly to a named agent, bypassing the router.
        /// </summary>
        /// <remarks>
        /// Use this when you already know which specialist should handle the request.
        /// Returns 404 if the agent name does not match any entry in configuration.
        /// Conversation history and the `includeTrace` flag work the same way as in
        /// <c>POST /api/v1/chat</c>.
        /// </remarks>
        /// <param name="agentName">Agent name exactly as declared in <c>AgentOrchestration:Agents[].Name</c> in configuration.</param>
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
