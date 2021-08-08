using System.Collections.Generic;

namespace Pazyn.DDD
{
    public abstract class Entity<T>
    {
        public T Id { get; protected set; }

        public override bool Equals(object obj)
        {
            if (obj is not Entity<T> other)
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

        public static bool operator ==(Entity<T> a, Entity<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity<T> a, Entity<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }
    }
}