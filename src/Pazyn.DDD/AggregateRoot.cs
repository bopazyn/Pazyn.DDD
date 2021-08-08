using System.Collections.Generic;
using MediatR;

namespace Pazyn.DDD
{
    public abstract class AggregateRoot<T> : Entity<T>
    {
        private readonly List<INotification> _domainEvents = new();
        public IEnumerable<INotification> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(INotification newEvent) =>
            _domainEvents.Add(newEvent);

        public void ClearDomainEvents() =>
            _domainEvents.Clear();
    }
}