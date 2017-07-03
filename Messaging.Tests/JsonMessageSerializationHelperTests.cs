using System;
using System.Diagnostics.CodeAnalysis;
using Messaging.Kafka;
using Xunit;

namespace Messaging.Tests
{
    public class JsonMessageSerializationHelperTests
    {
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        private sealed class Foo
        {
            private bool Equals(Foo other)
            {
                return Bar.Equals(other.Bar);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Foo) obj);
            }

            public override int GetHashCode()
            {
                return Bar.GetHashCode();
            }

            public Foo()
            {
                Bar = Guid.NewGuid();
            }

            [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
            [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
            public Guid Bar { get; set; }
        }

        [Fact]
        public void Deserialized_EqualsPreSerialized()
        {
            var input = new Foo();

            var sut = new JsonMessageSerializationHelper();
            var bytes = sut.Serialize(input);
            var output = sut.Deserialize(bytes);

            Assert.Equal(input, output);
        }
    }
}
