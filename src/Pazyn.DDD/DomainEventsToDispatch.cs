using System;
using MediatR;

namespace Pazyn.DDD
{
    internal class DomainEventsToDispatch
    {
        public INotification[] DomainEvents { get; init; }
        public Action ClearDomainEvents { get; init; }
    }
}