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
            const string brokerList = "localhost:9092";
            private readonly string testPrefix = $"{nameof(BasicIntegrationTests)}_{Guid.NewGuid()}";

            public EntityTopicConvention TestTopicConvention => entityType => $"{testPrefix}_{MessageConventions.DefaultTopic(entityType)}";

            public IServiceCollection ConfigureServices()
            {
                IServiceCollection services = new ServiceCollection();
                services.AddLogging();

                const string applicationName = nameof(BasicIntegrationTests);
                var writServices = new WritKafkaServices<string, object>(applicationName, new KafkaConfig
                {
                    BrokerList = brokerList,
                    AutoOffset = "smallest",
                    GroupId = applicationName
                });
                writServices.UseKeySerialization(new StringSerialization());
                writServices.UseObjectSerialization(new JsonSerialization());
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
            var testMessageHandler = new TestMessageHandler();
            serviceCollection.AddSingletonHandler<string, Test, TestMessageHandler>(testMessageHandler);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sent = new Test{ Id = 1, Name = "Foo" };
            using (var producer = serviceProvider.GetRequiredService<ConventionalObjectMessageProducer<string, object>>())
            {
                var producerTask = producer.ProduceAsync(sent);
                producerTask.GetAwaiter().GetResult();
            }

            var topicForType = _fixture.TestTopicConvention;
            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                consumer.Subscribe(topicForType(typeof(Test)));

                var dispatcher = serviceProvider.GetRequiredService<ObjectMessageDispatcher<string>>();
                dispatcher.DispatchMessagesFor(consumer);

                while (!testMessageHandler.Received.Any())
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }

            var received = testMessageHandler.Received.First();
            Assert.Equal(sent.Id, received.Id);
            Assert.Equal(sent.Name, received.Name);
        }

        class TestMessageHandler : ObjectMessageHandler<string, Test>
        {
            public List<Test> Received { get; } = new List<Test>();

            public override void Handle(Message<string, object> message, Test value)
            {
                Received.Add(value);
            }
        }
    }
}
