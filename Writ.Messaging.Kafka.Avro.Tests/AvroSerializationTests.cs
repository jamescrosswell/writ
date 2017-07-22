using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Xunit;

namespace Writ.Messaging.Kafka.Avro.Tests
{
    public class AvroSerializationTests
    {
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        [DataContract]
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
            [DataMember]
            public Guid Bar { get; set; }
        }

        private sealed class Fixture
        {
            public AvroSerialization GetSut()
            {
                var schemaTypeMap = new SchemaTypeMap();
                schemaTypeMap.RegisterTypeSchema<Foo>("AvroSerializationTests.Foo");
                schemaTypeMap.RegisterTypeSchema<Animal>("AvroSerializationTests.Animal");
                schemaTypeMap.RegisterTypeSchema<Color>("AvroSerializationTests.Color");
                schemaTypeMap.RegisterTypeSchema<Animal<Color>>("AvroSerializationTests.ColoredAnimal");
                schemaTypeMap.RegisterTypeSchema<MessageEnvelope<Foo>>("AvroSerializationTests.EnvelopedFoo");
                return new AvroSerialization(schemaTypeMap);
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Deserialized_EqualsPreSerialized()
        {
            var input = new Foo();

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(input);
            var output = sut.Deserialize(bytes);

            Assert.Equal(input, output);
        }

        [DataContract]
        class Color
        {
            [DataMember]
            public string Name { get; set; }
        }

        [DataContract]
        class Animal
        {
            [DataMember]
            public string Kind { get; set; }
            [DataMember]
            public Color Color { get; set; }
        }

        [Fact]
        public void Deserialize_deserializes_nested_objects()
        {

            var input = new Animal
            {
                Kind = "Horse",
                Color = new Color
                {
                    Name = "White"
                }
            };

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(input);
            var output = (Animal)sut.Deserialize(bytes);

            Assert.Equal(input.Kind, output.Kind);
            Assert.NotNull(output.Color);
            Assert.Equal(input.Color.Name, output.Color.Name);
        }

        [DataContract]
        class Animal<T>
        {
            [DataMember]
            public string Type { get; set; }
            [DataMember]
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

            var sut = _fixture.GetSut();
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

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(envelopedInput);
            var envelopedOutput = (MessageEnvelope<Foo>)sut.Deserialize(bytes);

            var output = handler.Open(envelopedOutput);
            Assert.Equal(input, output);
        }
    }
}

