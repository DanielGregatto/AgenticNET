using Azure.Identity;
using Domain.Configs;
using Microsoft.SemanticKernel;
using System;

namespace AgentInfrastructure.Providers
{
    internal static class AgentKernelFactory
    {
        internal static Kernel Create(
            AgentOptions agent,
            AgentOrchestrationOptions options)
        {
            var providerName = string.IsNullOrWhiteSpace(agent.Provider)
                ? options.DefaultProvider
                : agent.Provider;

            if (!options.Providers.TryGetValue(providerName, out var provider))
                throw new InvalidOperationException(
                    $"Agent '{agent.Name}' references provider '{providerName}' which is not configured under AgentOrchestration:Providers.");

            var builder = Kernel.CreateBuilder();

            switch (providerName)
            {
                case "AzureOpenAI":
                case "AzureOpenAI-EastUS2":
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: agent.DeploymentOrModel,
                        endpoint: provider.Endpoint,
                        credentials: new DefaultAzureCredential());
                    break;

                case "OpenAI":
                    throw new InvalidOperationException(
                        "OpenAI provider requires an API key and is not supported in Entra ID mode. Use an AzureOpenAI provider instead.");

                default:
                    throw new InvalidOperationException(
                        $"Unknown provider '{providerName}'. Supported values: AzureOpenAI, AzureOpenAI-EastUS2.");
            }

            return builder.Build();
        }
    }
}