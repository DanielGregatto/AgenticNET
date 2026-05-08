using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Plugins.Interfaces
{
    public interface IRAGPlugin
    {
        Task<string> SearchKnowledgeBase(string query, CancellationToken cancellationToken = default);
    }
}
