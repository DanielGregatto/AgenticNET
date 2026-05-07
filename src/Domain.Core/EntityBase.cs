using Domain.Core.Events.Interfaces;
using System;
using System.Collections.Generic;

namespace Domain.Core
{
    public abstract class EntityBase<T> : IHasDomainEvents where T : EntityBase<T>
    {
        protected EntityBase()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Deleted = false;
            _domainEvents = new List<object>();
        }
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Deleted { get; set; }

        public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

        private readonly List<object> _domainEvents;

        public override bool Equals(object obj)
        {
            var compareTo = obj as EntityBase<T>;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(EntityBase<T> a, EntityBase<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(EntityBase<T> a, EntityBase<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void RaiseDomainEvent(object evt)
        {
            _domainEvents.Add(evt);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}