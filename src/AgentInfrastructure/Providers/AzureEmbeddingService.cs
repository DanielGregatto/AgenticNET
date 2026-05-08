using AgentInfrastructure.Providers.Interfaces;
using Domain.Configs;
using Domain.Contracts.Common;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace AgentInfrastructure.Providers
{
    /// <summary>
    /// Provides an implementation of the IEmbeddingService interface that generates text embeddings using Azure OpenAI
    /// services.
    /// </summary>
    /// <remarks>This service acts as a wrapper around AzureOpenAITextEmbeddingGenerationService, enabling
    /// integration with Azure-hosted embedding models. It is typically configured via dependency injection using
    /// RAGSearchOptions and AgentOrchestrationOptions. Thread safety and performance characteristics depend on the
    /// underlying AzureOpenAITextEmbeddingGenerationService implementation.</remarks>
    public class AzureEmbeddingService : IEmbeddingService
    {
        private readonly AzureOpenAITextEmbeddingGenerationService _inner;

        public AzureEmbeddingService(IOptions<EmbeddingOptions> embeddingOptions)
        {
            var options = embeddingOptions.Value;

            _inner = new AzureOpenAITextEmbeddingGenerationService(
                options.Deployment,
                options.Endpoint,
                options.ApiKey);
        }

        /// <summary>
        /// Generates an embedding vector for the specified query string asynchronously.
        /// </summary>
        /// <remarks>The returned embedding can be used for tasks such as semantic search or similarity
        /// comparison. If the operation fails, the Result will indicate the error and no embedding will be
        /// returned.</remarks>
        /// <param name="query">The input text for which to generate an embedding vector. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with the
        /// generated embedding vector as a read-only memory of floats if successful; otherwise, contains error
        /// information.</returns>
        public async Task<Result<ReadOnlyMemory<float>>> GenerateEmbeddingAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                IList<ReadOnlyMemory<float>> results = await _inner.GenerateEmbeddingsAsync(
                    new List<string> { query },
                    kernel: null,
                    cancellationToken: cancellationToken);

                return Result<ReadOnlyMemory<float>>.Success(results[0]);
            }
            catch (Exception ex)
            {
                return Result<ReadOnlyMemory<float>>.Failure(
                    $"Failed to generate embedding: {ex.Message}", ErrorTypes.External);
            }
        }
    }
}
