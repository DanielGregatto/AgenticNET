using Domain.Core;

namespace Data.Events.Interfaces
{
    public sealed class EntityInsertedEvent<T> : IEntityInsertedEvent<T>  where T : EntityBase<T>
    {
        public EntityInsertedEvent(T entity) => Entity = entity;
        public T Entity { get; set; }
    }
}
