using System.Collections.Generic;
using System.Linq;
using MediatR;

namespace Pazyn.DDD
{
    public abstract class AggregateRoot<T> : Entity<T> where T : struct
    {
        private readonly List<INotification> domainEvents = new List<INotification>();
        public virtual IReadOnlyList<INotification> DomainEvents => domainEvents.ToList();

        protected virtual void AddDomainEvent(INotification newEvent) =>
            domainEvents.Add(newEvent);

        public virtual void ClearDomainEvents() =>
            domainEvents.Clear();
    }
}