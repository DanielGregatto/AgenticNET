using AgentInfrastructure.Providers.Interfaces;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Domain.Configs;
using Domain.Contracts.Common;
using Domain.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers
{
    /// <summary>
    /// Provides a retrieval-augmented generation (RAG) provider that uses Azure AI Search to retrieve relevant
    /// information based on vector embeddings.
    /// </summary>
    /// <remarks>This class integrates with Azure AI Search and an embedding service to perform semantic
    /// search over indexed documents. It is typically used to enhance generative AI applications by supplying
    /// contextually relevant information from a knowledge base. Thread safety is not guaranteed; create separate
    /// instances for concurrent use.</remarks>
    public class AzureAiSearchRagProvider : IRAGProvider
    {
        private readonly RAGSearchOptions _options;
        private readonly IEmbeddingService _embeddingService;
        private readonly SearchClient _searchClient;

        public AzureAiSearchRagProvider(
            IOptions<RAGSearchOptions> options,
            IEmbeddingService embeddingService)
        {
            _options = options.Value;
            _embeddingService = embeddingService;

            var credential = new ChainedTokenCredential(
                new AzureCliCredential(),
                new DefaultAzureCredential());

            _searchClient = new SearchClient(
                new Uri(_options.Endpoint),
                _options.IndexName,
                credential);
        }

        /// <summary>
        /// Executes a semantic search against the knowledge base using the specified query and returns the formatted
        /// results asynchronously.
        /// </summary>
        /// <remarks>The returned string includes the title and content of each matching entry, separated
        /// by delimiters. If the search fails due to an external service error, the result will indicate failure with
        /// an appropriate error message.</remarks>
        /// <param name="query">The search query to use for retrieving relevant information from the knowledge base. Cannot be null or
        /// empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the search operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result object with a formatted
        /// string of search results if successful; if no relevant information is found, the result indicates not found;
        /// if an error occurs, the result contains error details.</returns>
        public async Task<Result<string>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

                if (embeddingResult.IsFailure)
                    return Result<string>.Failure(embeddingResult.Errors.ToArray());

                var vectorQuery = new VectorizedQuery(embeddingResult.Data)
                {
                    KNearestNeighborsCount = _options.TopK,
                    Fields = { _options.VectorFieldName }
                };

                var searchOptions = new SearchOptions
                {
                    VectorSearch = new() { Queries = { vectorQuery } },
                    Size = _options.TopK,
                    Select = { "chunk_id", "parent_id", "title", "chunk" }
                };

                var response = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions, cancellationToken);

                var sb = new StringBuilder();
                var count = 0;

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var doc = result.Document;
                    var title = doc.TryGetValue("title", out var t) ? t?.ToString() : "Entry";
                    var content = doc.TryGetValue("chunk", out var c) ? c?.ToString() : string.Empty;

                    if (count > 0) sb.AppendLine("\n---\n");
                    sb.AppendLine($"**{title}**");
                    sb.AppendLine(content);
                    count++;
                }

                return count == 0
                    ? Result<string>.NotFound("No relevant information found in the knowledge base.")
                    : Result<string>.Success(sb.ToString());
            }
            catch (RequestFailedException ex)
            {
                return Result<string>.Failure(
                    $"Azure AI Search request failed: {ex.Message}", ErrorTypes.External);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure(
                    $"Unexpected error while searching the knowledge base: {ex.Message}", ErrorTypes.External);
            }
        }
    }
}
