using System;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;
using Sample.Domain;
using Sample.Domain.Accounts;
using Sample.Domain.Deposits;
using Sample.Domain.Payments;
using Sample.EventStore;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;

namespace Sample.Client
{
    public class Program
    {
        private static ILogger _logger;
        private static bool _cancelled = false;
        private static Random Random => new Random();
        private static PersonNameGenerator People => new PersonNameGenerator(Random);

        private static readonly string ApplicationName;
        private const string BrokerList = "localhost:9092";

        static Program()
        {
            ApplicationName = $"Sample.Client.{Guid.NewGuid()}";
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
                consumer.Subscribe(aggregates.GetEventTopics(true));

                _logger.LogInformation("Starting simulation...");

                Console.CancelKeyPress += OnCancelKeyPress;
                while (!_cancelled)
                {
                    // Randomly produce events on accounts roughly once per second... assuming a poll time of 100ms
                    switch (Random.Next(1, 31))
                    {
                        case 1:
                            _logger.LogDebug("Creating account...");
                            var createAccount = new CreateAccount(Guid.NewGuid(), People.GenerateRandomFirstAndLastName());
                            producer.ProduceAsync(createAccount).GetAwaiter().GetResult();
                            break;
                        case var eventId when new[] { 2,3 }.Contains(eventId):
                            var count = state.Accounts.Count();
                            if (count > 0)
                            {
                                var skip = Random.Next(0, count);
                                const int take = 1;
                                var account = state.Accounts.Find(x => true, skip, take).Single();
                                if (eventId == 2)
                                {
                                    _logger.LogDebug("Making a deposit...");
                                    var makeDeposit = new MakeDeposit(account.Id, Random.Next(1, 100));
                                    producer.ProduceAsync(makeDeposit).GetAwaiter().GetResult();
                                }
                                else
                                {
                                    _logger.LogDebug("Attempting payment...");
                                    var makePayment = new MakePayment(account.Id, Random.Next(1, 30));
                                    producer.ProduceAsync(makePayment).GetAwaiter().GetResult();
                                }
                            }
                            break;
                    }

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
            services.AddEventHandlers();

            services.AddSampleSchemaConventions();

            return services.BuildServiceProvider();
        }
    }
}