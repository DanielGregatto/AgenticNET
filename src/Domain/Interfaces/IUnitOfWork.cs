using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern - manages database transactions and domain event publishing
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all changes to the database and publishes domain events
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Number of entities written to the database</returns>
        Task<int> CommitAsync(CancellationToken ct = default);

        /// <summary>
        /// Saves changes to the database without publishing domain events (rare use case)
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Number of entities written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
