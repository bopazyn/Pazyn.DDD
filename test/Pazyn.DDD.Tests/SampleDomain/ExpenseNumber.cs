using System;
using Pazyn.DDD.SingleValue;

namespace Pazyn.DDD.Tests.SampleDomain
{
    public class ExpenseNumber : SingleValueObject<String>
    {
        public ExpenseNumber(String value) : base(value)
        {
        }
    }
}