namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseType : Entity<int>
    {
        public static readonly ExpenseType Hobby = new(1, nameof(Hobby));
        public static readonly ExpenseType Food = new(2, nameof(Food));
        public static readonly ExpenseType Bills = new(3, nameof(Bills));

        private ExpenseType()
        {
        }

        private ExpenseType(int id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Name { get; }
    }
}