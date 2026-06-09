using AgentInfrastructure.Plugins.Interfaces;
using AgentInfrastructure.Providers;
using AgentInfrastructure.Providers.Interfaces;
using Domain.Configs;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace AgentInfrastructure.Plugins
{
    public class RagPluginFactory : IRagPluginFactory
    {
        private readonly RAGSearchOptions _options;
        private readonly IEmbeddingService _embeddingService;

        public RagPluginFactory(IOptions<RAGSearchOptions> options, IEmbeddingService embeddingService)
        {
            _options = options.Value;
            _embeddingService = embeddingService;
        }

        public IRAGPlugin Create(string indexKey)
        {
            var exists = _options.Catalogs.Any(c =>
                string.Equals(c, indexKey, StringComparison.OrdinalIgnoreCase));

            if (!exists)
                throw new InvalidOperationException(
                    $"RAG catalog '{indexKey}' is not registered under RAGSearch:Catalogs. " +
                    $"Available: {string.Join(", ", _options.Catalogs)}");

            var indexName = $"rag-{indexKey.ToLowerInvariant()}";

            var provider = new AzureAiSearchRagProvider(
                _options.Endpoint,
                indexName,
                _options.VectorFieldName,
                _options.TopK,
                _embeddingService);

            return new RAGPlugin(provider);
        }
    }
}
