using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace Messaging.Kafka
{
    /// <summary>
    /// Base class for writing objects to kafka. 
    /// </summary>
    public class ObjectMessageProducer : IObjectMessageProducer
    {
        private readonly Producer<string, object> _producer;

        /// <summary>
        ///  Saves us from having to proxy individual methods of the producer in decorators
        /// </summary>
        public ProducerProxy Internal { get; }    

        /// <summary>
        /// Creates an ConventionalObjectMessageProducer
        /// </summary>
        /// <param name="config">Configuration settings to use for the producer (e.g. the brokers list)</param>
        /// <param name="serializer">The object serializer that should be used to write the message</param>
        public ObjectMessageProducer(KafkaConfig config, ISerializer<object> serializer)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            _producer = new Producer<string, object>(config, new StringSerializer(Encoding.UTF8), serializer);
            Internal = new ProducerProxy(_producer);
        }

        public Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return _producer.ProduceAsync(topic, key, value);
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
