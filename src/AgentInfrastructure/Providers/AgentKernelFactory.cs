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

            switch (providerName)
            {
                case "AzureAI":
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: agent.DeploymentOrModel,
                        endpoint: provider.Endpoint,
                        credentials: new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential()));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown provider '{providerName}'. Supported values: AzureAI.");
            }

            return Task.FromResult(builder.Build());
        }
    }
}
