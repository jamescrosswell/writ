using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;

namespace OptimisticKafka
{
    class Program
    {
        static void Main(string[] args)
        {
            const string brokerList = "localhost:9092";
            const string topicName = "test";

            Console.WriteLine("Hello World!");

            var config = new Dictionary<string, object> { { "bootstrap.servers", brokerList } };
            
            using (var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                Console.WriteLine($"{producer.Name} producing on {topicName}.");

                var key = Guid.NewGuid().ToString();
                var deposit = new MakeDeposit(10m);
                var value = JsonConvert.SerializeObject(deposit); 
                var deliveryReport = producer.ProduceAsync(topicName, key, value);
                deliveryReport.ContinueWith(task =>
                {
                    Console.WriteLine($"Partition: {task.Result.Partition}, Offset: {task.Result.Offset}");
                });

                // Tasks are not waited on synchronously (ContinueWith is not synchronous),
                // so it's possible they may still in progress here.
                producer.Flush(TimeSpan.FromSeconds(10));
            }

            // Wait for a key press to finishe
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        class MakeDeposit
        {
            public MakeDeposit(decimal amount)
            {
                this.Amount = amount;
            }

            public decimal Amount { get; }
        }
    }
}