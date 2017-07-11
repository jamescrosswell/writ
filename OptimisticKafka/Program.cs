using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Writ.Messaging;
using Writ.Messaging.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OptimisticKafka
{
    internal class Program
    {
        private const string BrokerList = "localhost:9092";

        private static void Main(string[] args)
        {
            // Create a temporary topic for these tests
            string testTopic = $"OptimisticKafka_{Guid.NewGuid()}";

            //setup our DI
            var serviceCollection = new ServiceCollection();
            var serviceProvider = ConfigureServices(serviceCollection, entityType => testTopic);

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Starting application");

            using (var producer = serviceProvider.GetRequiredService<ConventionalObjectMessageProducer<string, object>>())
            {
                using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
                {
                    consumer.Subscribe(testTopic);

                    var dispatcher = serviceProvider.GetRequiredService<ObjectMessageDispatcher<string>>();
                    dispatcher.DispatchMessagesFor(consumer);

                    var cancelled = false;
                    Console.CancelKeyPress += (_, e) =>
                    {
                        e.Cancel = true; // prevent the process from terminating.
                        cancelled = true;
                    };

                    Console.WriteLine("Ctrl-C to exit.");
                    while (!cancelled)
                    {
                        var producerTask = producer.ProduceAsync(new MakeDeposit(10m));
                        var deliveryReport = producerTask.ContinueWith(LogDeliveryReport);
                        deliveryReport.GetAwaiter().GetResult();

                        consumer.Poll(TimeSpan.FromMilliseconds(100));
                    }

                }

                void LogDeliveryReport(Task<Message<string, object>> task)
                {
                    logger.LogDebug($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset}");
                }
            }
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services, EntityTopicConvention topicConvention)
        {
            services.AddLogging();

            const string applicationName = "OptimisticKafka";
            var writServices = new WritKafkaServices<string, object>(applicationName, new KafkaConfig
            {
                BrokerList = BrokerList,
                AutoOffset = "smallest",
                GroupId = applicationName
            });
            writServices.UseKeySerializers(new StringSerializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8));
            writServices.UseObjectSerialization(new JsonMessageSerializationHelper());
            writServices.UseConventions(topicConvention ?? MessageConventions.Topic, MessageConventions.Key);
            services.AddWrit(writServices);

            services.AddHandler<string, MakeDeposit, MakeDepositMessageHandler>();

            return services.BuildServiceProvider();
        }
    }

    class MakeDepositMessageHandler : ObjectMessageHandler<string, MakeDeposit>
    {
        public override void Handle(Message<string, object> message, MakeDeposit value)
        {
            Console.WriteLine($"Topic: {message.Topic} Partition: {message.Partition} Offset: {message.Offset}");
            Console.WriteLine($"Deposit received for ${value.Amount}");
        }
    }
}