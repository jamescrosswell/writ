using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Messaging;
using Messaging.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OptimisticKafka
{
    class Program
    {
        const string brokerList = "localhost:9092";

        static void Main(string[] args)
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

            var producerFactory = serviceProvider.GetService<EntityMessageProducerFactory>();
            using (var producer = producerFactory.CreateProducer<MakeDeposit>())
            {
                var value = new MakeDeposit(10m);
                var deliveryReport = producer.ProduceAsync(value);
                var task2 = deliveryReport.ContinueWith(task =>
                {
                    logger.LogDebug($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset}");
                });
                task2.GetAwaiter().GetResult();
            }

            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {                
                consumer.Subscribe(new[] { nameof(MakeDeposit) });
                logger.LogDebug($"Consumer {consumer.Name} subscribed to { string.Join(",", consumer.Subscription) }");

                var cancelled = false;
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; // prevent the process from terminating.
                    cancelled = true;
                };

                Console.WriteLine("Ctrl-C to exit.");
                while (!cancelled)
                {
                    if (consumer.Consume(out Message<string, object> msg, TimeSpan.FromSeconds(1)))
                    {
                        Console.WriteLine($"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset}");
                        Console.WriteLine($"{msg.Value}");
                    }
                }
            }
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddScoped<ICorrelationIdProvider, DefaultCorrelationProvider>();

            services.AddTransient<IEnvelopeHandler>(p => new EnvelopeHandler(
                applicationName: "OptimisticKafka", correlationProvider: p.GetService<ICorrelationIdProvider>()));

            services.AddSingleton(p => new KafkaConfig(new Dictionary<string, object>
            {
                { "group.id", "OptimisticKafka" },
                { "auto.offset.reset", "smallest" },
                { "bootstrap.servers", brokerList }
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

            services.AddTransient<Consumer<string, object>>(p =>
                new Consumer<string, object>(
                    p.GetRequiredService <KafkaConfig>(),
                    p.GetRequiredService<IDeserializer<string>>(),
                    p.GetRequiredService<IDeserializer<object>>()));

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

            public IObjectMessageProducer<TMessage> CreateProducer<TMessage>()
                where TMessage: Entity
            {
                return new ConventionalObjectMessageProducer<TMessage>(
                    _serviceProvider.GetService<EnvelopedObjectMessageProducer>(),
                    MessageConventions.Key,
                    MessageConventions.Topic,
                    _serviceProvider.GetService<ILogger<ConventionalObjectMessageProducer<TMessage>>>()
                );
            }
        }
    }
}