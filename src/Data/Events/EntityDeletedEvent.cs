using Data.Events.Interfaces;
using Domain.Core;

namespace Data.Events
{
    public sealed class EntityDeletedEvent<T> : IEntityDeletedEvent<T> where T : EntityBase<T>
    {
        public EntityDeletedEvent(T entity) => Entity = entity;
        public T Entity { get; set; }
    }
}
