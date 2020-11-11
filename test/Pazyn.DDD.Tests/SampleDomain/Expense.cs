using System;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class Expense : AggregateRoot<Int32>
    {
        // ReSharper disable once UnusedMember.Local
        private Expense()
        {
        }

        public Expense(ExpenseNumber number, ExpenseType type)
        {
            Number = number;
            Type = type;
        }

        public ExpenseNumber Number { get; private set; }
        public ExpenseType Type { get; set; }

        public void AddDomainEvent()
        {
            AddDomainEvent(new ExpenseEvent
            {
                Number = Number,
            });
        }
    }
}