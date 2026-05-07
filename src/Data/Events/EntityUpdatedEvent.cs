using Domain.Core;

namespace Data.Events.Interfaces
{
    public sealed class EntityUpdatedEvent<T> : IEntityUpdatedEvent<T> where T : EntityBase<T>
    {
        public EntityUpdatedEvent(T entity) => Entity = entity;
        public T Entity { get; set; }
    }
}
