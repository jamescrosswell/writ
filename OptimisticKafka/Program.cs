using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Messaging;
using Messaging.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OptimisticKafka
{
    internal class Program
    {
        private const string BrokerList = "localhost:9092";

        private static void Main(string[] args)
        {
            //setup our DI
            var serviceCollection = new ServiceCollection();
            var serviceProvider = ConfigureServices(serviceCollection);

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();
            logger.LogDebug("Starting application");

            // Create a temporary topic for these tests
            string testTopic = $"OptimisticKafka_{Guid.NewGuid()}";
            string TopicConvention(Type entityType) => testTopic;

            var producerFactory = serviceProvider.GetRequiredService<EntityMessageProducerFactory>();
            using (var producer = producerFactory.CreateProducer<MakeDeposit>(TopicConvention))
            {
                using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
                {
                    consumer.Subscribe(testTopic);

                    var dispatcher = serviceProvider.GetRequiredService<ObjectMessageDispatcher>();
                    dispatcher.DispatchMessagesFor(consumer);

                    var cancelled = false;
                    Console.CancelKeyPress += (_, e) => {
                        e.Cancel = true; // prevent the process from terminating.
                        cancelled = true;
                    };

                    Console.WriteLine("Ctrl-C to exit.");
                    while (!cancelled)
                    {
                        var producerTask = producer.ProduceAsync(new MakeDeposit(10m));
                        var deliveryReport  = producerTask.ContinueWith(LogDeliveryReport);
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

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddScoped<CorrelationProvider>(p => () => Guid.NewGuid().ToString());

            services.AddTransient<IEnvelopeHandler>(p => new EnvelopeHandler(
                applicationName: "OptimisticKafka", correlationProvider: p.GetService<CorrelationProvider>())
                );

            services.AddSingleton(p => new KafkaConfig(new Dictionary<string, object>
            {
                { "group.id", "OptimisticKafka" },
                { "auto.offset.reset", "smallest" },
                { "bootstrap.servers", BrokerList }
            }));

            services.AddSingleton<ISerializer<string>>(p => new StringSerializer(Encoding.UTF8));
            services.AddSingleton<IDeserializer<string>>(p => new StringDeserializer(Encoding.UTF8));
            services.AddSingleton<JsonMessageSerializationHelper>();
            services.AddSingleton<ISerializer<object>>(p => p.GetRequiredService<JsonMessageSerializationHelper>());
            services.AddSingleton<IDeserializer<object>>(p => p.GetRequiredService<JsonMessageSerializationHelper>());

            services.AddTransient<ISerializingProducer<string, object>>(
                p => new Producer<string, object>(
                    p.GetService<KafkaConfig>(),
                    p.GetService<ISerializer<string>>(), 
                    p.GetService<ISerializer<object>>()
                    ));

            // Register decorator chain ConventionalObjectMessageProducer → EnvelopedObjectMessageProducer → ObjectMessageProducer
            services.AddTransient<ObjectMessageProducer>();
            services.AddTransient(p => new EnvelopedObjectMessageProducer(
                p.GetService<ObjectMessageProducer>(), p.GetService<IEnvelopeHandler>()));
            services.AddSingleton<EntityMessageProducerFactory>();

            services.AddTransient(p =>
                new Consumer<string, object>(
                    p.GetRequiredService <KafkaConfig>(),
                    p.GetRequiredService<IDeserializer<string>>(),
                    p.GetRequiredService<IDeserializer<object>>()));

            services.AddTransient<ObjectMessageDispatcher>();
            services.AddTransient<IObjectMessageHandler<MakeDeposit>, MakeDepositMessageHandler>();
            services.AddTransient<IMessageHandler<string, object>, EnvelopedObjectMessageHandler<MakeDeposit>>();

            return services.BuildServiceProvider();
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class EntityMessageProducerFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public EntityMessageProducerFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public IObjectMessageProducer<TMessage> CreateProducer<TMessage>(EntityTopicConvention topicConvention)
                where TMessage: class, IEntity
            {
                return new ConventionalObjectMessageProducer<TMessage>(
                    _serviceProvider.GetService<EnvelopedObjectMessageProducer>(),
                    MessageConventions.Key,
                    topicConvention,
                    _serviceProvider.GetService<ILogger<ConventionalObjectMessageProducer<TMessage>>>()
                );
            }
        }
    }

    class MakeDepositMessageHandler : ObjectMessageHandler<MakeDeposit>
    {
        public override void Handle(Message<string, object> message, MakeDeposit value)
        {
            Console.WriteLine($"Topic: {message.Topic} Partition: {message.Partition} Offset: {message.Offset}");
            Console.WriteLine($"Deposit received for ${value.Amount}");
        }
    }
}