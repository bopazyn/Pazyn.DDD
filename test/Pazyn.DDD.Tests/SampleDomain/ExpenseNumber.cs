using System;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public record ExpenseNumber
    {
        public String Value { get; init; }

        public ExpenseNumber(String value) => Value = value;
    }
}