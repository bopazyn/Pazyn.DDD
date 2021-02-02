using System;
using MediatR;

namespace Pazyn.DDD
{
    internal class DomainEventsToDispatch
    {
        public INotification[] DomainEvents { get; set; }
        public Action ClearDomainEvents { get; set; }
    }
}