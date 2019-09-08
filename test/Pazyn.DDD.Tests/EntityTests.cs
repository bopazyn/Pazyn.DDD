using System;
using Xunit;

namespace Pazyn.DDD.Tests
{
    public class EntityTests
    {
        internal class ExampleEntity : Entity<Int32>
        {
            public ExampleEntity()
            {
            }

            public ExampleEntity(Int32 id) : this() => Id = id;
        }

        [Fact]
        public void Equals_AnotherHeapAddress()
        {
            var e1 = new ExampleEntity();
            var e2 = new ExampleEntity();

            Assert.NotEqual(e1, e2);
            Assert.False(e1 == e2);
            Assert.True(e1 != e2);
        }

        [Fact]
        public void Equals_BothNull()
        {
            ExampleEntity e1 = null;
            ExampleEntity e2 = null;

            Assert.Equal(e1, e2);
            Assert.True(e1 == e2);
            Assert.False(e1 != e2);
        }

        [Fact]
        public void Equals_DifferentIds()
        {
            var e1 = new ExampleEntity(1);
            var e2 = new ExampleEntity(2);

            Assert.NotEqual(e1, e2);
            Assert.False(e1 == e2);
            Assert.True(e1 != e2);
        }

        [Fact]
        public void Equals_SameHeapAddress()
        {
            var e1 = new ExampleEntity();
            var e2 = e1;

            Assert.Equal(e1, e2);
            Assert.True(e1 == e2);
            Assert.False(e1 != e2);
        }

        [Fact]
        public void Equals_IdAndNull()
        {
            var e1 = new ExampleEntity(1);
            ExampleEntity e2 = null;

            Assert.NotEqual(e1, e2);
            Assert.False(e1 == e2);
            Assert.True(e1 != e2);
        }

        [Fact]
        public void Equals_NewHeapAddressAndNull()
        {
            ExampleEntity e1 = null;
            var e2 = new ExampleEntity();

            Assert.NotEqual(e1, e2);
            Assert.False(e1 == e2);
            Assert.True(e1 != e2);
        }

        [Fact]
        public void Equals_NewHeapAddressAndId()
        {
            var e1 = new ExampleEntity();
            var e2 = new ExampleEntity(1);

            Assert.NotEqual(e1, e2);
            Assert.False(e1 == e2);
            Assert.True(e1 != e2);
        }
    }
}