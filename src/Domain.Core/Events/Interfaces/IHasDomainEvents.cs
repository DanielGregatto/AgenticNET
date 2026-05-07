using System.Collections.Generic;

namespace Domain.Core.Events.Interfaces
{
    /// <summary>
    /// Marker interface for entities that support domain events
    /// </summary>
    public interface IHasDomainEvents
    {
        public IReadOnlyCollection<object> DomainEvents { get; }
        public void RaiseDomainEvent(object evt);
        public void ClearDomainEvents();
    }
}
