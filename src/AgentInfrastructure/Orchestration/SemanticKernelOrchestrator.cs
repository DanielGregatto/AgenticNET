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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Orchestration
{
    public class SemanticKernelOrchestrator : IAgentOrchestrator
    {
        private readonly AgentOrchestrationOptions _options;
        private readonly ILogger<SemanticKernelOrchestrator> _logger;
        private readonly IConversationStore _conversationStore;
        private readonly IProductCatalogPlugin _productCatalogPlugin;
        private readonly IRAGPlugin _ragPlugin;

        public SemanticKernelOrchestrator(
            IOptions<AgentOrchestrationOptions> options,
            ILogger<SemanticKernelOrchestrator> logger,
            IConversationStore conversationStore,
            IProductCatalogPlugin productCatalogPlugin,
            IRAGPlugin ragPlugin)
        {
            _options = options.Value;
            _logger = logger;
            _conversationStore = conversationStore;
            _productCatalogPlugin = productCatalogPlugin;
            _ragPlugin = ragPlugin;
        }

        public IReadOnlyList<AgentOptions> GetRegisteredAgents() =>
            _options.Agents.AsReadOnly();

        public async Task<Result<AgentResponse>> SendMessageAsync(
            string agentName,
            string userMessage,
            string conversationId,
            CancellationToken cancellationToken = default)
        {
            var agentConfig = _options.Agents.FirstOrDefault(a => string.Equals(a.Name, agentName, StringComparison.OrdinalIgnoreCase));

            if (agentConfig is null)
                return Result<AgentResponse>.NotFound($"Agent '{agentName}' is not registered.");

            if (!_options.Providers.ContainsKey(agentConfig.Provider ?? _options.DefaultProvider))
                return Result<AgentResponse>.Failure(
                    $"Provider '{agentConfig.Provider ?? _options.DefaultProvider}' is not configured under AgentOrchestration:Providers.");

            _logger.LogInformation(
                "Routing message to agent {AgentName} on conversation {ConversationId}",
                agentName, conversationId);

            var kernel = AgentKernelFactory.Create(agentConfig, _options);

            foreach (var pluginName in agentConfig.Plugins)
                kernel.Plugins.AddFromObject(ResolvePlugin(pluginName));

            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var history = _conversationStore.GetOrCreate(
                conversationId,
                agentConfig.SystemPrompt ?? string.Empty);

            history.AddUserMessage(userMessage);

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
                history.AddAssistantMessage(content);

                _logger.LogInformation(
                    "Agent {AgentName} completed response for conversation {ConversationId}",
                    agentName, conversationId);

                return Result<AgentResponse>.Success(new AgentResponse(
                    AgentName: agentConfig.Name,
                    ConversationId: conversationId,
                    Content: content,
                    Timestamp: DateTimeOffset.UtcNow));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting response from agent '{AgentName}' for conversation {ConversationId}",
                    agentName, conversationId);

                return Result<AgentResponse>.Failure(
                    $"Failed to get response from agent '{agentName}'.", ErrorTypes.External);
            }
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
                    classifierHistory,
                    cancellationToken: cancellationToken);

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
            var routeResult = await RouteAsync(userMessage, cancellationToken);

            if (routeResult.IsFailure)
            {
                if (!canUseDefaultAgent)
                    return Result<AgentResponse>.Failure(routeResult.Errors.ToArray());

                _logger.LogWarning(
                    "Routing failed ({Error}). Falling back to default agent '{DefaultAgent}'.",
                    routeResult.Errors.FirstOrDefault()?.Message,
                    _options.DefaultAgentName);

                return await SendMessageAsync(_options.DefaultAgentName, userMessage, conversationId, cancellationToken);
            }

            return await SendMessageAsync(routeResult.Data, userMessage, conversationId, cancellationToken);
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
                    throw new InvalidOperationException($"Plugin '{name}' is not registered. Available plugins: ProductCatalog, RAG.");
            }
        }
    }
}