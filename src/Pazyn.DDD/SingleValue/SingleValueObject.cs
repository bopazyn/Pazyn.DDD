using System;
using System.Collections.Generic;

namespace Pazyn.DDD.SingleValue
{
    public abstract class SingleValueObject<T> : ValueObject
    {
        public T Value { get; private set; }

        protected SingleValueObject(T value)
        {
            Value = value;
        }

        public override String ToString() => Value.ToString();

        protected override IEnumerable<Object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}