using Azure.AI.OpenAI;
using Azure.Identity;
using Domain.Configs;
using Microsoft.SemanticKernel;
using System;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers
{
    internal static class AgentKernelFactory
    {
        internal static Task<Kernel> CreateAsync(
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
            var credential = new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential());

            switch (providerName)
            {
                case "AzureAI":
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: agent.DeploymentOrModel,
                        endpoint: provider.Endpoint,
                        credentials: credential);
                    break;

                case "AzureAIFoundry":
                    var foundryClient = new AzureOpenAIClient(
                        new Uri(provider.Endpoint),
                        credential);
                    builder.AddAzureOpenAIChatCompletion(agent.DeploymentOrModel, foundryClient);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown provider '{providerName}'. Supported values: AzureAI, AzureAIFoundry.");
            }

            return Task.FromResult(builder.Build());
        }
    }
}
