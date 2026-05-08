using Domain.Contracts.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentInfrastructure.Providers.Interfaces
{
    public interface IEmbeddingService
    {
        Task<Result<ReadOnlyMemory<float>>> GenerateEmbeddingAsync(string query, CancellationToken cancellationToken = default);
    }
}
