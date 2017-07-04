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
        private readonly ISerializingProducer<string, object> _producer;

        /// <summary>
        ///  <inheritdoc cref="IObjectMessageProducer"/>
        /// </summary>
        public string Name => _producer.Name;

        public ObjectMessageProducer(KafkaConfig config, ISerializingProducer<string, object> producer)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
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
            (_producer as IDisposable)?.Dispose();
        }
    }
}
