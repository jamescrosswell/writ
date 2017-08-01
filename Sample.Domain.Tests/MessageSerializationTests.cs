using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Sample.Domain.Accounts;
using Sample.Domain.Deposits;
using Sample.Domain.Payments;
using Writ.Messaging;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;
using Xunit;

namespace Sample.Domain.Tests
{
    public class MessageSerializationTests
    {

        private sealed class Fixture
        {
            public static MessageOffset SampleOffset => new MessageOffset("foo", 7, 101);

            public JsonSerialization GetSut()
            {
                var schemaTypeMap = new SchemaTypeMap();

                schemaTypeMap.RegisterTypeSchema<CreateAccount>("MessageSerializationTests.CreateAccount");
                schemaTypeMap.RegisterTypeSchema<MakeDeposit>("MessageSerializationTests.MakeDeposit");
                schemaTypeMap.RegisterTypeSchema<MakePayment>("MessageSerializationTests.MakePayment");

                schemaTypeMap.RegisterTypeSchema<AccountCreated>("MessageSerializationTests.AccountCreated");
                schemaTypeMap.RegisterTypeSchema<DepositMade>("MessageSerializationTests.DepositMade");
                schemaTypeMap.RegisterTypeSchema<PaymentMade>("MessageSerializationTests.PaymentMade");

                return new JsonSerialization(schemaTypeMap);
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public static IEnumerable<object[]> GetSerializableEntities()
        {
            yield return new[] { new AccountCreated(Fixture.SampleOffset, Guid.NewGuid(), "Emily Downs") };
            yield return new[] { new DepositMade(Fixture.SampleOffset, Guid.NewGuid(), 37) };
            yield return new[] { new PaymentMade(Fixture.SampleOffset, Guid.NewGuid(), 79) };
            yield return new[] { new CreateAccount(Guid.NewGuid(), "Emily Downs") };
            yield return new[] { new MakeDeposit(Guid.NewGuid(), 37) };
            yield return new[] { new MakePayment(Guid.NewGuid(), 79) };
        }

        [Theory]
        [MemberData(nameof(GetSerializableEntities))]
        public void DeserializedEqualsSerialized(object input)
        {
            var sut = _fixture.GetSut();
            var bytes = sut.Serialize(input);
            var output = sut.Deserialize(bytes);

            Assert.Equal(input, output);
        }

        [Fact]
        public void AllCommandsAndEventsAreTested()
        {
            // Find all the classes in the domain that implement IEvent<,>
            var expectedTypes = GetConcreteDomainTypes()
                .Where(x => x.GetInterfaces().Any(i => 
                    i.GetGenericTypeDefinition() == typeof(IEvent<,>)
                    || i.GetGenericTypeDefinition() == typeof(ICommand<,>)
                    )
                );

            // Serialize and Deserialize each, and then check the output == input
            var typesWeTest = GetSerializableEntities().SelectMany(_ => _).Select(x => x.GetType().GetTypeInfo()).ToList();
            foreach (var expected in expectedTypes)
            {
                Assert.Contains(expected, typesWeTest);
            }
        }

        private static IEnumerable<TypeInfo> GetConcreteDomainTypes()
        {
            var sampleAssembly = Assembly.Load(new AssemblyName("Sample.Domain"));
            return sampleAssembly.DefinedTypes.Where(x => !x.IsAbstract && !x.IsInterface);
        }
    }
}
