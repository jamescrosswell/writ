using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.EventStore;
using System;
using System.Linq;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;

namespace Sample.CommandProcessor
{
    public class Program
    {
        private static ILogger _logger;
        private static bool _cancelled = false;

        private static readonly string ApplicationName;
        private const string BrokerList = "localhost:9092";

        static Program()
        {
            ApplicationName = $"Sample.CommandProcessor.{Guid.NewGuid()}";
        }

        private static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole()
                .CreateLogger(nameof(Program));

            var state = serviceProvider.GetRequiredService<ApplicationState>();

            Console.CancelKeyPress += OnCancelKeyPress;

            LoadApplicationState(serviceProvider);

            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                var aggregates = serviceProvider.GetRequiredService<AggregateRootRegistry>();
                consumer.Subscribe(aggregates.GetCommandTopics());

                _logger.LogInformation("Listening for commands...");

                while (!_cancelled)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }


        }

        /// <summary>
        /// Rebuilds the application state by applying historic events
        /// </summary>
        /// <param name="serviceProvider"></param>
        private static void LoadApplicationState(IServiceProvider serviceProvider)
        {
            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                var aggregates = serviceProvider.GetRequiredService<AggregateRootRegistry>();
                consumer.Subscribe(aggregates.GetEventTopics(false));

                var metaData = consumer.GetMetadata(true);
                var topicPartitions =
                    metaData
                        .Topics
                        .Where(t => aggregates.GetEventTopics(false).Contains(t.Topic))
                        .SelectMany(t => t.Partitions.Select(p => new TopicPartition(t.Topic, p.PartitionId)))
                        .ToList();
                var watermarks = topicPartitions
                    .Select(p => new { TopicPartition = p, WatermarkOffsets = consumer.QueryWatermarkOffsets(p)})
                    .ToDictionary(
                        p => p.TopicPartition,
                        p => p.WatermarkOffsets
                    );

                _logger.LogInformation("Loading application state...");

                while (!_cancelled)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));

                    var partitionOffsets = consumer.Position(topicPartitions);
                    if (partitionOffsets.All(p => p.Offset == watermarks[p.TopicPartition].High))
                        break;
                }

                _logger.LogInformation("Application state loaded successfully!");
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