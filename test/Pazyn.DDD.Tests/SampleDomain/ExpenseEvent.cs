using MediatR;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseEvent : INotification
    {
        public ExpenseNumber Number { get; init; }
    }
}