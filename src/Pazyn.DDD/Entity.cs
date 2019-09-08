using System;
using System.Collections.Generic;

namespace Pazyn.DDD
{
    public abstract class Entity<T> where T : struct
    {
        public T Id { get; protected set; }

        public override Boolean Equals(Object obj)
        {
            if (!(obj is Entity<T> other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            var comparer = Comparer<T>.Default;
            if (comparer.Compare(Id, default) == 0 || comparer.Compare(other.Id, default) == 0)
                return false;

            return comparer.Compare(Id, other.Id) == 0;
        }

        public static Boolean operator ==(Entity<T> a, Entity<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static Boolean operator !=(Entity<T> a, Entity<T> b)
        {
            return !(a == b);
        }

        public override Int32 GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }
    }
}