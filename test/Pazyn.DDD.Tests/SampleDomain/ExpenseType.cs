using System;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseType : Entity<Int32>
    {
        public static readonly ExpenseType Hobby = new ExpenseType(1, nameof(Hobby));
        public static readonly ExpenseType Food = new ExpenseType(2, nameof(Food));
        public static readonly ExpenseType Bills = new ExpenseType(3, nameof(Bills));

        private ExpenseType()
        {
        }

        private ExpenseType(Int32 id, String name) : this()
        {
            Id = id;
            Name = name;
        }

        public String Name { get; private set; }
    }
}