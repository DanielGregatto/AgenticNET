using AgentInfrastructure.Conversations;
using AgentInfrastructure.Plugins;
using AgentInfrastructure.Plugins.Interfaces;
using AgentInfrastructure.Providers;
using Domain.Configs;
using Domain.Contracts.Agent;
using Domain.Contracts.Common;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Services.Features.Conversation.Commands.GetOrCreate;
using Services.Features.Conversation.Commands.SaveTurn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Orchestration
{
    public class SemanticKernelOrchestrator : IAgentOrchestrator
    {
        private readonly AgentOrchestrationOptions _options;
        private readonly ILogger<SemanticKernelOrchestrator> _logger;
        private readonly IProductCatalogPlugin _productCatalogPlugin;
        private readonly IRAGPlugin _ragPlugin;
        private readonly IMediatorHandler _mediator;
        private readonly IUser _user;

        public SemanticKernelOrchestrator(
            IOptions<AgentOrchestrationOptions> options,
            ILogger<SemanticKernelOrchestrator> logger,
            IProductCatalogPlugin productCatalogPlugin,
            IRAGPlugin ragPlugin,
            IMediatorHandler mediator,
            IUser user)
        {
            _options = options.Value;
            _logger = logger;
            _productCatalogPlugin = productCatalogPlugin;
            _ragPlugin = ragPlugin;
            _mediator = mediator;
            _user = user;
        }

        public IReadOnlyList<AgentOptions> GetRegisteredAgents() =>
            _options.Agents.AsReadOnly();

        public async Task<Result<AgentResponse>> SendMessageAsync(
            string agentName,
            string userMessage,
            string conversationId,
            CancellationToken cancellationToken = default)
        {
            var agentConfig = _options.Agents.FirstOrDefault(a =>
                string.Equals(a.Name, agentName, StringComparison.OrdinalIgnoreCase));

            if (agentConfig is null)
                return Result<AgentResponse>.NotFound($"Agent '{agentName}' is not registered.");

            if (!_options.Providers.ContainsKey(agentConfig.Provider ?? _options.DefaultProvider))
                return Result<AgentResponse>.Failure(
                    $"Provider '{agentConfig.Provider ?? _options.DefaultProvider}' is not configured.");

            if (!Guid.TryParse(conversationId, out var conversationGuid))
                return Result<AgentResponse>.Failure($"Invalid conversationId format: '{conversationId}'.");

            return await SendMessageCoreAsync(agentConfig, userMessage, conversationGuid, new TraceContext(), cancellationToken);
        }

        public async Task<Result<string>> RouteAsync(
            string userMessage,
            CancellationToken cancellationToken = default)
        {
            var routerConfig = _options.Agents.FirstOrDefault(a =>
                string.Equals(a.Name, _options.RouterAgentName, StringComparison.OrdinalIgnoreCase));

            if (routerConfig is null)
                return Result<string>.Failure(
                    $"Router agent '{_options.RouterAgentName}' is not registered in AgentOrchestration:Agents.");

            var candidates = GetRegisteredAgents()
                .Where(a => !string.Equals(a.Name, _options.RouterAgentName, StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(a.Name, _options.DefaultAgentName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!candidates.Any())
                return Result<string>.Failure("No specialist agents are registered for routing.");

            var agentList = string.Join("\n", candidates.Select(a =>
                $"- Name: {a.Name} | Description: {a.Description}"));

            var systemPrompt =
                "You are an intelligent agent router. Based on the user message, return ONLY the agent Name of the most appropriate agent.\n\n" +
                $"Available agents:\n{agentList}\n\n" +
                "Reply with ONLY the exact Name value from the list above. No explanation. No punctuation.";

            _logger.LogInformation("Classifying message via RouterAgent '{RouterAgent}'", _options.RouterAgentName);

            try
            {
                var routerKernel = AgentKernelFactory.Create(routerConfig, _options);
                var chatService = routerKernel.GetRequiredService<IChatCompletionService>();

                var classifierHistory = new ChatHistory(systemPrompt);
                classifierHistory.AddUserMessage(userMessage);

                var response = await chatService.GetChatMessageContentAsync(
                    classifierHistory, cancellationToken: cancellationToken);

                var resolvedName = response.Content?.Trim();

                var matched = candidates.FirstOrDefault(a =>
                    string.Equals(a.Name, resolvedName, StringComparison.OrdinalIgnoreCase));

                if (matched is null)
                    return Result<string>.Failure(
                        $"Router returned unknown agent '{resolvedName}'. " +
                        $"Available: {string.Join(", ", candidates.Select(a => a.Name))}.");

                _logger.LogInformation("RouterAgent resolved '{ResolvedAgent}'", matched.Name);

                return Result<string>.Success(matched.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during agent classification");
                return Result<string>.Failure("Failed to classify the message.", ErrorTypes.External);
            }
        }

        public async Task<Result<AgentResponse>> RouteAndSendAsync(
            string userMessage,
            string conversationId,
            bool canUseDefaultAgent = true,
            CancellationToken cancellationToken = default)
        {
            if (!Guid.TryParse(conversationId, out var conversationGuid))
                return Result<AgentResponse>.Failure($"Invalid conversationId format: '{conversationId}'.");

            var traceContext = new TraceContext();

            var routeResult = await RouteAsync(userMessage, cancellationToken);

            string resolvedAgentName;

            if (routeResult.IsFailure)
            {
                if (!canUseDefaultAgent)
                    return Result<AgentResponse>.Failure(routeResult.Errors.ToArray());

                _logger.LogWarning(
                    "Routing failed ({Error}). Falling back to default agent '{DefaultAgent}'.",
                    routeResult.Errors.FirstOrDefault()?.Message,
                    _options.DefaultAgentName);

                resolvedAgentName = _options.DefaultAgentName;
            }
            else
            {
                resolvedAgentName = routeResult.Data;
            }

            traceContext.Add(TraceStepType.RouterDecision, new
            {
                SelectedAgent = resolvedAgentName,
                RoutingFailed = routeResult.IsFailure,
                UsedDefault = routeResult.IsFailure && canUseDefaultAgent
            });

            var agentConfig = _options.Agents.FirstOrDefault(a =>
                string.Equals(a.Name, resolvedAgentName, StringComparison.OrdinalIgnoreCase));

            if (agentConfig is null)
                return Result<AgentResponse>.NotFound($"Agent '{resolvedAgentName}' is not registered.");

            return await SendMessageCoreAsync(agentConfig, userMessage, conversationGuid, traceContext, cancellationToken);
        }

        private async Task<Result<AgentResponse>> SendMessageCoreAsync(
            AgentOptions agentConfig,
            string userMessage,
            Guid conversationGuid,
            TraceContext traceContext,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Routing message to agent {AgentName} on conversation {ConversationId}",
                agentConfig.Name, conversationGuid);

            var historyResult = await _mediator.SendCommand(
                new GetOrCreateConversationCommand { ConversationId = conversationGuid });

            if (historyResult.IsFailure)
                return Result<AgentResponse>.Failure(historyResult.Errors.ToArray());

            var history = BuildChatHistory(agentConfig.SystemPrompt, historyResult.Data.Messages);
            history.AddUserMessage(userMessage);

            var kernel = AgentKernelFactory.Create(agentConfig, _options);

            foreach (var pluginName in agentConfig.Plugins)
                kernel.Plugins.AddFromObject(ResolvePlugin(pluginName));

            kernel.FunctionInvocationFilters.Add(new TracingFunctionInvocationFilter(traceContext));

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var executionSettings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            try
            {
                var response = await chatService.GetChatMessageContentAsync(
                    history,
                    executionSettings: executionSettings,
                    kernel: kernel,
                    cancellationToken: cancellationToken);

                var content = response.Content ?? string.Empty;

                if (agentConfig.Review.Required && !string.IsNullOrWhiteSpace(agentConfig.Review.AgentReviewerName))
                    await RunReviewerAsync(agentConfig, userMessage, content, traceContext, cancellationToken);

                var saveResult = await _mediator.SendCommand(new SaveConversationTurnCommand
                {
                    ConversationId = conversationGuid,
                    UserMessage = userMessage,
                    AgentName = agentConfig.Name,
                    AssistantMessage = content,
                    TraceSteps = traceContext.Steps.ToList()
                });

                if (saveResult.IsFailure)
                    _logger.LogWarning(
                        "Failed to persist turn for conversation {ConversationId}: {Errors}",
                        conversationGuid,
                        string.Join(", ", saveResult.Errors.Select(e => e.Message)));

                _logger.LogInformation(
                    "Agent {AgentName} completed response for conversation {ConversationId}",
                    agentConfig.Name, conversationGuid);

                return Result<AgentResponse>.Success(new AgentResponse(
                    AgentName: agentConfig.Name,
                    ConversationId: conversationGuid.ToString(),
                    Content: content,
                    Timestamp: DateTimeOffset.UtcNow,
                    Trace: traceContext.Steps));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while getting response from agent '{AgentName}' for conversation {ConversationId}",
                    agentConfig.Name, conversationGuid);

                return Result<AgentResponse>.Failure(
                    $"Failed to get response from agent '{agentConfig.Name}'.", ErrorTypes.External);
            }
        }

        private async Task RunReviewerAsync(
            AgentOptions agentConfig,
            string userMessage,
            string assistantAnswer,
            TraceContext traceContext,
            CancellationToken cancellationToken)
        {
            var reviewerConfig = _options.Agents.FirstOrDefault(a =>
                string.Equals(a.Name, agentConfig.Review.AgentReviewerName, StringComparison.OrdinalIgnoreCase));

            if (reviewerConfig is null)
            {
                _logger.LogWarning(
                    "Reviewer agent '{ReviewerName}' not found for agent '{AgentName}'.",
                    agentConfig.Review.AgentReviewerName, agentConfig.Name);
                return;
            }

            try
            {
                var reviewerKernel = AgentKernelFactory.Create(reviewerConfig, _options);
                var chatService = reviewerKernel.GetRequiredService<IChatCompletionService>();

                var reviewerHistory = new ChatHistory(reviewerConfig.SystemPrompt ?? string.Empty);
                reviewerHistory.AddUserMessage(
                    $"Question: {userMessage}\n\nAnswer: {assistantAnswer}");

                var response = await chatService.GetChatMessageContentAsync(
                    reviewerHistory, cancellationToken: cancellationToken);

                var reviewerOutput = TryParseReviewerOutput(response.Content);

                if (reviewerOutput is null)
                {
                    _logger.LogWarning(
                        "Reviewer agent '{ReviewerName}' returned non-parseable response.",
                        agentConfig.Review.AgentReviewerName);
                    return;
                }

                traceContext.Add(TraceStepType.ReviewerDecision, new
                {
                    ReviewerAgent = agentConfig.Review.AgentReviewerName,
                    reviewerOutput.IsValid,
                    reviewerOutput.Confidence,
                    BelowThreshold = agentConfig.Review.ConfidenceScore.HasValue
                        && reviewerOutput.Confidence < agentConfig.Review.ConfidenceScore.Value,
                    Threshold = agentConfig.Review.ConfidenceScore,
                    reviewerOutput.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error running reviewer agent '{ReviewerName}'.",
                    agentConfig.Review.AgentReviewerName);
            }
        }

        private static ReviewerOutput TryParseReviewerOutput(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                var start = content.IndexOf('{');
                var end = content.LastIndexOf('}');
                if (start < 0 || end <= start) return null;

                return JsonSerializer.Deserialize<ReviewerOutput>(content[start..(end + 1)]);
            }
            catch
            {
                return null;
            }
        }

        private static ChatHistory BuildChatHistory(string systemPrompt, IEnumerable<Services.Contracts.Results.ConversationMessageResult> messages)
        {
            var history = new ChatHistory(systemPrompt ?? string.Empty);

            foreach (var msg in messages)
            {
                if (msg.Role == "User") history.AddUserMessage(msg.Content);
                else if (msg.Role == "Assistant") history.AddAssistantMessage(msg.Content);
            }

            return history;
        }

        private object ResolvePlugin(string name)
        {
            switch (name)
            {
                case "ProductCatalog":
                    return _productCatalogPlugin;
                case "RAG":
                    return _ragPlugin;
                default:
                    throw new InvalidOperationException(
                        $"Plugin '{name}' is not registered. Available plugins: ProductCatalog, RAG.");
            }
        }
    }
}
