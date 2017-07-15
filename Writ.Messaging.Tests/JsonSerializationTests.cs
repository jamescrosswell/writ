using System;
using System.Diagnostics.CodeAnalysis;
using Writ.Messaging.Kafka.Serialization;
using Xunit;

namespace Writ.Messaging.Tests
{
    public class JsonSerializationTests
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

            var sut = new JsonSerialization();
            var bytes = sut.Serialize(input);
            var output = sut.Deserialize(bytes);

            Assert.Equal(input, output);
        }

        class Color
        {
            public string Name { get; set; }
        }

        class Animal
        {
            public string Type { get; set; }

            public Color Color { get; set; }
        }

        [Fact]
        public void Deserialize_deserializes_nested_objects()
        {

            var input = new Animal
            {
                Type = "Horse",
                Color = new Color
                {
                    Name = "White"
                }
            };

            var sut = new JsonSerialization();
            var bytes = sut.Serialize(input);
            var output = (Animal)sut.Deserialize(bytes);

            Assert.Equal(input.Type, output.Type);
            Assert.NotNull(output.Color);
            Assert.Equal(input.Color.Name, output.Color.Name);
        }

        class Animal<T>
        {
            public string Type { get; set; }

            public T Attribute { get; set; }
        }

        [Fact]
        public void Deserialize_deserializes_generic_nested_objects()
        {

            var input = new Animal<Color>
            {
                Type = "Horse",
                Attribute = new Color
                {
                    Name = "White"
                }
            };

            var sut = new JsonSerialization();
            var bytes = sut.Serialize(input);
            var output = (Animal<Color>)sut.Deserialize(bytes);

            Assert.Equal(input.Type, output.Type);
            Assert.NotNull(output.Attribute);
            Assert.Equal(input.Attribute.Name, output.Attribute.Name);
        }

        [Fact]
        public void Deserialize_deserializes_enveloped_objects()
        {
            var handler = new EnvelopeHandler("JsonMessageSerializationHelperTests", () => Guid.NewGuid().ToString());

            var input = new Foo();
            var envelopedInput = (MessageEnvelope<Foo>)handler.Stuff(input);

            var sut = new JsonSerialization();
            var bytes = sut.Serialize(envelopedInput);
            var envelopedOutput = (MessageEnvelope<Foo>)sut.Deserialize(bytes);

            var output = handler.Open(envelopedOutput);
            Assert.Equal(input, output);
        }
    }
}

