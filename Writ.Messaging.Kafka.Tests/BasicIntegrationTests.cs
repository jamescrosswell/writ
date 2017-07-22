using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Writ.Messaging.Kafka.Serialization;
using Xunit;

namespace Writ.Messaging.Kafka.Tests
{

    public class BasicIntegrationTests
    {
        private sealed class Fixture
        {
            const string ApplicationName = nameof(BasicIntegrationTests);
            const string BrokerList = "localhost:9092";
            private readonly string _testPrefix = $"{nameof(BasicIntegrationTests)}_{Guid.NewGuid()}";
            public EntityTopicConvention TestTopicConvention =>
                entityType => $"{_testPrefix}_{MessageConventions.DefaultTopic(entityType)}";

            public IServiceCollection ConfigureServices()
            {
                IServiceCollection services = new ServiceCollection();
                services.AddLogging();

                var typeMap = new SchemaTypeMap();
                typeMap.RegisterTypeSchema<Test>("Test");
                typeMap.RegisterTypeSchema<MessageEnvelope<Test>>("TestEnvelope");
                services.AddSingleton<ISchemaTypeMap>(typeMap);

                var writServices = new WritKafkaServices<string, object>(ApplicationName, new KafkaConfig
                {
                    BrokerList = BrokerList,
                    AutoOffset = "smallest",
                    GroupId = ApplicationName
                });
                writServices.UseKeySerialization<string, object, StringSerialization>();
                writServices.UseObjectSerialization<string, object, JsonSerialization>();
                writServices.UseTopicConvention(TestTopicConvention);
                services.AddWrit(writServices);

                return services;
            }
        }

        private readonly Fixture _fixture = new Fixture();

        class Test
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Received_Equals_Sent()
        {
            //setup our DI
            var serviceCollection = _fixture.ConfigureServices();
            serviceCollection.AddSingleton<ApplicationState>();
            serviceCollection.AddHandler<string, Test, TestMessageHandler>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sent = new Test{ Id = 1, Name = "Foo" };
            using (var producer = serviceProvider.GetRequiredService<ConventionalObjectMessageProducer<string, object>>())
            {
                var producerTask = producer.ProduceAsync(sent);
                producerTask.GetAwaiter().GetResult();
            }

            var topicForType = _fixture.TestTopicConvention;
            var applicationState = serviceProvider.GetRequiredService<ApplicationState>();
            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                consumer.Subscribe(topicForType(typeof(Test)));

                while (!applicationState.Received.Any())
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }

            var received = applicationState.Received.First();
            Assert.Equal(sent.Id, received.Id);
            Assert.Equal(sent.Name, received.Name);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ApplicationState
        {
            public List<Test> Received { get; } = new List<Test>();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestMessageHandler : ObjectMessageHandler<string, Test>
        {
            private readonly ApplicationState _applicationState;

            public TestMessageHandler(ApplicationState applicationState)
            {
                _applicationState = applicationState;
            }

            public override void Handle(Message<string, object> message, Test value)
            {
                _applicationState.Received.Add(value);
            }
        }
    }
}
