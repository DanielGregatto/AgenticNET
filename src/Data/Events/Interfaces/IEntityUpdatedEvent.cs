using Domain.Core;
using MediatR;

namespace Data.Events.Interfaces
{
    public interface IEntityUpdatedEvent<T> : INotification where T : EntityBase<T>
    {
        T Entity { get; set; }
    }
}
