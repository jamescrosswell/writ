using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.EventStore;
using System;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;

namespace Sample.CommandProcessor
{
    public class Program
    {
        private static ILogger _logger;
        private static bool _cancelled = false;

        private static string ApplicationName = "Sample.CommandProcessor";
        private const string BrokerList = "localhost:9092";

        static Program()
        {
            ApplicationName = $"{Guid.NewGuid()}";
        }

        private static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole()
                .CreateLogger(nameof(Program));

            var state = serviceProvider.GetRequiredService<ApplicationState>();

            using (var producer = serviceProvider.GetRequiredService<ConventionalObjectMessageProducer<string, object>>())
            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                var aggregates = serviceProvider.GetRequiredService<AggregateRootRegistry>();
                consumer.Subscribe(aggregates.GetCommandTopics());

                _logger.LogInformation("Listening for commands...");

                Console.CancelKeyPress += OnCancelKeyPress;
                while (!_cancelled)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }


        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _cancelled = true;
        }

        public static IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();

            var aggregates = services.AddSampleAggregates();

            var writServices = new WritKafkaServices<string, object>(ApplicationName, new KafkaConfig
            {
                BrokerList = BrokerList,
                AutoOffset = "smallest",
                GroupId = ApplicationName
            });
            writServices.UseKeySerialization<string, object, StringSerialization>();
            writServices.UseObjectSerialization<string, object, JsonSerialization>();
            writServices.UseKeyConvention(MessageConventions.StringKeyConvention<object>());
            writServices.UseTopicConvention(aggregates.TopicConvention);
            services.AddWrit(writServices);

            services.AddApplicationState();
            services.AddCommandHandlers();
            services.AddEventHandlers();

            services.AddSampleSchemaConventions();

            return services.BuildServiceProvider();
        }
    }
}