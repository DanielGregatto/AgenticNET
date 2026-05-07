using AgentInfrastructure.Conversations;
using AgentInfrastructure.Plugins;
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

        public SemanticKernelOrchestrator(
            IOptions<AgentOrchestrationOptions> options,
            ILogger<SemanticKernelOrchestrator> logger,
            IConversationStore conversationStore,
            IProductCatalogPlugin productCatalogPlugin)
        {
            _options = options.Value;
            _logger = logger;
            _conversationStore = conversationStore;
            _productCatalogPlugin = productCatalogPlugin;
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

        private object ResolvePlugin(string name)
        {
            switch (name)
            {
                case "ProductCatalog":
                    return _productCatalogPlugin;
                default:
                    throw new InvalidOperationException($"Plugin '{name}' is not registered. Available plugins: ProductCatalog.");
            }
        }
    }
}