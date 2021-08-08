using System;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public readonly struct ExpenseId : IComparable<ExpenseId>, IEquatable<ExpenseId>
    {
        public int Value { get; }

        public ExpenseId(int value)
        {
            Value = value;
        }

        public int CompareTo(ExpenseId other) => other.Value.CompareTo(Value);
        public bool Equals(ExpenseId other) => other.Value.Equals(Value);
        //
        // public static implicit operator ExpenseId(int orderId) => new(orderId);
        // public static implicit operator int(ExpenseId orderId) => orderId.Value;
        
        public static explicit operator ExpenseId(int orderId) => new(orderId);
        public static explicit operator int(ExpenseId orderId) => orderId.Value;
    }
}