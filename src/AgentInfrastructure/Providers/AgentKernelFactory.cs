using Azure.Core;
using Azure.Identity;
using Domain.Configs;
using Microsoft.SemanticKernel;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers
{
    internal static class AgentKernelFactory
    {
        internal static async Task<Kernel> CreateAsync(
            AgentOptions agent,
            AgentOrchestrationOptions options)
        {
            var providerName = string.IsNullOrWhiteSpace(agent.Provider)
                ? options.DefaultProvider
                : agent.Provider;

            if (!options.Providers.TryGetValue(providerName, out var provider))
                throw new InvalidOperationException(
                    $"Agent '{agent.Name}' references provider '{providerName}' which is not configured under AgentOrchestration:Providers.");

            if (providerName.StartsWith("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
                await VerifyAzureAccessAsync(providerName, provider, agent.DeploymentOrModel);

            var builder = Kernel.CreateBuilder();

            switch (providerName)
            {
                case "AzureOpenAI":
                case "AzureOpenAI-EastUS2":
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: agent.DeploymentOrModel,
                        endpoint: provider.Endpoint,
                        credentials: new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential()));
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

        private static async Task VerifyAzureAccessAsync(
            string providerName,
            AgentProviderOptions provider,
            string deploymentOrModel)
        {
            try
            {
                var credential = new ChainedTokenCredential(new AzureCliCredential(), new DefaultAzureCredential());
                var ctx = new TokenRequestContext(["https://cognitiveservices.azure.com/.default"]);
                var token = await credential.GetTokenAsync(ctx);

                using var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Token);

                var url = $"{provider.Endpoint.TrimEnd('/')}/openai/models?api-version=2024-10-21";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException(
                        $"Azure OpenAI endpoint for provider '{providerName}' " +
                        $"is not accessible: {(int)response.StatusCode} {response.StatusCode}. " +
                        $"Ensure 'az login' is done and the account has the Cognitive Services OpenAI User role.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Azure OpenAI access check failed for provider '{providerName}': {ex.Message}", ex);
            }
        }
    }
}
