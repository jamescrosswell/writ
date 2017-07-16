using System;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RandomNameGeneratorLibrary;
using Sample.Domain.Accounts;
using Sample.Domain.Deposit;
using Sample.Domain.Payment;
using Sample.EventStore;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;
using Writ.Messaging.Kafka.Serialization;

namespace Sample.Client
{
    class Program
    {
        private static bool _cancelled = false;
        private static Random Random => new Random();
        private static PersonNameGenerator People => new PersonNameGenerator(Random);

        const string ApplicationName = "Sample.Client";
        const string BrokerList = "localhost:9092";

        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var state = serviceProvider.GetRequiredService<ApplicationState>();

            using (var producer = serviceProvider.GetRequiredService<ConventionalObjectMessageProducer<string, object>>())
            using (var consumer = serviceProvider.GetRequiredService<Consumer<string, object>>())
            {
                var aggregates = serviceProvider.GetRequiredService<AggregateRootRegistry>();
                consumer.Subscribe(aggregates.GetEventTopics(true));

                Console.CancelKeyPress += OnCancelKeyPress;
                while (!_cancelled)
                {
                    // Randomly produce events on accounts roughly once per second... assuming a poll time of 100ms
                    switch (Random.Next(1, 31))
                    {
                        case 1:
                            var createAccount = new CreateAccount(Guid.NewGuid(), People.GenerateRandomFirstAndLastName());
                            producer.ProduceAsync(createAccount).GetAwaiter().GetResult();
                            break;
                        case var eventId when new[] { 2,3 }.Contains(eventId):
                            var count = state.Accounts.Count();
                            if (count > 0)
                            {
                                var skip = Random.Next(0, count);
                                var account = state.Accounts.Find(x => true, skip).Single();
                                if (eventId == 2)
                                {
                                    var makeDeposit = new MakeDeposit(account.Id, Random.Next(1, 100));
                                    producer.ProduceAsync(makeDeposit)
                                        .GetAwaiter()
                                        .GetResult();
                                }
                                else
                                {
                                    var makePayment = new MakePayment(account.Id, Random.Next(1, 30));
                                    producer.ProduceAsync(makePayment)
                                        .GetAwaiter()
                                        .GetResult();
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

            var aggregates = new AggregateRootRegistry();
            aggregates.RegisterAggregate<Account>();
            services.AddSingleton(aggregates);

            var writServices = new WritKafkaServices<string, object>(ApplicationName, new KafkaConfig
            {
                BrokerList = BrokerList,
                AutoOffset = "smallest",
                GroupId = ApplicationName
            });
            writServices.UseKeySerialization(new StringSerialization());
            writServices.UseObjectSerialization(new JsonSerialization());
            writServices.UseKeyConvention(MessageConventions.StringKeyConvention<object>());
            writServices.UseTopicConvention(aggregates.TopicConvention);
            services.AddWrit(writServices);

            services.AddEventStore();

            return services.BuildServiceProvider();
        }
    }
}