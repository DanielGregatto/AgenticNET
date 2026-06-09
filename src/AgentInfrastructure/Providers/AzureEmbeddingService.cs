using AgentInfrastructure.Providers.Interfaces;
using Azure.AI.OpenAI;
using Azure.Identity;
using Domain.Configs;
using Domain.Contracts.Common;
using Domain.Enums;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers
{
    public class AzureEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _client;

        public AzureEmbeddingService(IOptions<EmbeddingOptions> embeddingOptions)
        {
            var options = embeddingOptions.Value;
            var credential = new ChainedTokenCredential(
                new AzureCliCredential(),
                new DefaultAzureCredential());
            _client = new AzureOpenAIClient(new Uri(options.Endpoint), credential)
                .GetEmbeddingClient(options.Deployment);
        }

        public async Task<Result<ReadOnlyMemory<float>>> GenerateEmbeddingAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                OpenAIEmbedding result = await _client.GenerateEmbeddingAsync(
                    query, cancellationToken: cancellationToken);
                return Result<ReadOnlyMemory<float>>.Success(result.ToFloats());
            }
            catch (Exception ex)
            {
                return Result<ReadOnlyMemory<float>>.Failure(
                    $"Failed to generate embedding: {ex.Message}", ErrorTypes.External);
            }
        }
    }
}
