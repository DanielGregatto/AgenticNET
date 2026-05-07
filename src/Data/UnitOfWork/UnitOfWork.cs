using Data.Context;
using Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work implementation - coordinates database transactions and domain event publishing
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;

        public UnitOfWork(AppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<int> CommitAsync(CancellationToken ct = default)
        {
            var result = await _context.SaveChangesAsync(ct);

            if (result > 0)
            {
                var domainEvents = _context.GetDomainEvents();

                foreach (var domainEvent in domainEvents)
                    await _mediator.Publish(domainEvent, ct);

                _context.ClearDomainEvents();
            }

            return result;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}
