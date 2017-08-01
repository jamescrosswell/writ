using System;
using System.Diagnostics.CodeAnalysis;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;
using Xunit;

namespace Writ.Messaging.Kafka.Tests
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

        private sealed class Fixture
        {
            public JsonSerialization GetSut()
            {
                var schemaTypeMap = new SchemaTypeMap();
                schemaTypeMap.RegisterTypeSchema<Foo>("JsonSerializationTests.Foo");
                schemaTypeMap.RegisterTypeSchema<Animal>("JsonSerializationTests.Animal");
                schemaTypeMap.RegisterTypeSchema<Color>("JsonSerializationTests.Color");
                schemaTypeMap.RegisterTypeSchema<Animal<Color>>("JsonSerializationTests.ColoredAnimal");
                schemaTypeMap.RegisterTypeSchema<MessageEnvelope<Foo>>("JsonSerializationTests.EnvelopedFoo");
                schemaTypeMap.RegisterTypeSchema<CommandFailure<CreateAnimal, Animal, int>>("CreateAnimaFailure");
                return new JsonSerialization(schemaTypeMap);
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

        class Color
        {
            public string Name { get; set; }
        }

        class Animal : IAggregateRoot<int>
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public Color Color { get; set; }
        }

        [Fact]
        public void Deserialize_deserializes_nested_objects()
        {
            var input = new Animal
            {
                Id = 1,
                Type = "Horse",
                Color = new Color
                {
                    Name = "White"
                }
            };

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(input);
            var output = (Animal)sut.Deserialize(bytes);

            Assert.Equal(input.Id, output.Id);
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
            var handler = new EnvelopeHandler("JsonSerializationTests", () => Guid.NewGuid().ToString());

            var input = new Foo();
            var envelopedInput = (MessageEnvelope<Foo>)handler.Stuff(input);

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(envelopedInput);
            var envelopedOutput = (MessageEnvelope<Foo>)sut.Deserialize(bytes);

            var output = handler.Open(envelopedOutput);
            Assert.Equal(input, output);
        }

        class CreateAnimal : ICommand<Animal, int>
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Deserialize_deserializes_command_failures()
        {
            var command = new CreateAnimal{Id = 123};
            var input = new CommandFailure<CreateAnimal, Animal, int>(command, "testing 123");

            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(input);
            var output = sut.Deserialize(bytes) as CommandFailure<CreateAnimal, Animal, int>;

            Assert.NotNull(output?.Command);
            Assert.Equal(input.Reason, output.Reason);
            Assert.Equal(input.Command.Id, output.Command.Id);
        }
    }
}

