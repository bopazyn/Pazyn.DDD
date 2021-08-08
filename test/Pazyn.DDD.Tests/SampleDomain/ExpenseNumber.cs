namespace Pazyn.DDD.Tests.SampleDomain
{
    public record ExpenseNumber
    {
        public string Value { get; }

        public ExpenseNumber(string value) => Value = value;
    }
}