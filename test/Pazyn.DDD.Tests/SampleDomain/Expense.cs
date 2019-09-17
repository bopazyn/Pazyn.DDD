using System;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class Expense : AggregateRoot<Int32>
    {
        private Expense()
        {
        }

        public Expense(ExpenseType type) : this()
        {
            SetType(type);
        }

        public ExpenseType Type { get; private set; }

        public void AddDomainEvent()
        {
            AddDomainEvent(new ExpenseEvent());
        }

        public void SetType(ExpenseType type)
        {
            Type = type;
        }
    }
}