using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Confluent.Kafka.Serialization;
using Messaging;
using Messaging.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OptimisticKafka
{
    class Program
    {
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
                deliveryReport.ContinueWith(task =>
                {
                    logger.LogDebug($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset}");
                });

                // Tasks are not waited on synchronously (ContinueWith is not synchronous),
                // so it's possible they may still in progress here.
                producer.Internal.Flush(TimeSpan.FromSeconds(10));
            }

            // Wait for a key press to finish
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddScoped<ICorrelationIdProvider, DefaultCorrelationProvider>();

            services.AddTransient<IEnvelopeHandler>(p => new EnvelopeHandler(
                applicationName: "OptimisticKafka", correlationProvider: p.GetService<ICorrelationIdProvider>()));

            const string brokerList = "localhost:9092";
            services.AddSingleton(p => new KafkaConfig(new Dictionary<string, object> { { "bootstrap.servers", brokerList } }));

            services.AddTransient<ISerializer<object>, JsonMessageSerializationHelper>();
            services.AddTransient<IDeserializer<object>, JsonMessageSerializationHelper>();

            // Register decorator chain ConventionalObjectMessageProducer → EnvelopedObjectMessageProducer → ObjectMessageProducer
            services.AddTransient<ObjectMessageProducer>();
            services.AddTransient(p => new EnvelopedObjectMessageProducer(
                p.GetService<ObjectMessageProducer>(), p.GetService<IEnvelopeHandler>()));
            services.AddSingleton<EntityMessageProducerFactory>();

            return services.BuildServiceProvider();
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        class EntityMessageProducerFactory
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
                    _serviceProvider.GetService<ILoggerFactory>()
                );
            }
        }
    }
}