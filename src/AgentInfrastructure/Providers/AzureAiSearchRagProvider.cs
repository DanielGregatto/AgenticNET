using AgentInfrastructure.Providers.Interfaces;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Domain.Contracts.Common;
using Domain.Enums;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers
{
    public class AzureAiSearchRagProvider : IRAGProvider
    {
        private readonly string _vectorFieldName;
        private readonly int _topK;
        private readonly IEmbeddingService _embeddingService;
        private readonly SearchClient _searchClient;

        public AzureAiSearchRagProvider(
            string endpoint,
            string indexName,
            string vectorFieldName,
            int topK,
            IEmbeddingService embeddingService)
        {
            _vectorFieldName = vectorFieldName;
            _topK = topK;
            _embeddingService = embeddingService;

            var credential = new ChainedTokenCredential(
                new AzureCliCredential(),
                new DefaultAzureCredential());

            _searchClient = new SearchClient(new Uri(endpoint), indexName, credential);
        }

        public async Task<Result<string>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

                if (embeddingResult.IsFailure)
                    return Result<string>.Failure(embeddingResult.Errors.ToArray());

                var vectorQuery = new VectorizedQuery(embeddingResult.Data)
                {
                    KNearestNeighborsCount = _topK,
                    Fields = { _vectorFieldName }
                };

                var searchOptions = new SearchOptions
                {
                    VectorSearch = new() { Queries = { vectorQuery } },
                    Size = _topK,
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
