using Domain.Contracts.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers.Interfaces
{
    public interface IRAGProvider
    {
        Task<Result<string>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
