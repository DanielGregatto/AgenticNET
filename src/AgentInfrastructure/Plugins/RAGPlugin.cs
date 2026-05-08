using AgentInfrastructure.Plugins.Interfaces;
using AgentInfrastructure.Providers.Interfaces;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Plugins
{
    public class RAGPlugin : IRAGPlugin
    {
        private readonly IRAGProvider _provider;

        public RAGPlugin(IRAGProvider provider)
        {
            _provider = provider;
        }

        [KernelFunction, Description("Search the knowledge base for relevant information")]
        public async Task<string> SearchKnowledgeBase(
            [Description("The question or topic to search for")] string query,
            CancellationToken cancellationToken = default)
        {
            var result = await _provider.SearchAsync(query, cancellationToken);

            if (result.IsFailure)
                return $"Could not retrieve knowledge base results: {string.Join(", ", result.Errors.Select(e => e.Message))}";

            return result.Data;
        }
    }
}
