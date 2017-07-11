using Confluent.Kafka;
using System;
using System.Threading.Tasks;

namespace Writ.Messaging.Kafka
{
    /// <summary>
    /// Base class for writing objects to kafka. 
    /// </summary>
    public class ObjectMessageProducer<TKey> : IObjectMessageProducer<TKey>
    {
        private readonly ISerializingProducer<TKey, object> _producer;

        /// <summary>
        ///  <inheritdoc cref="IObjectMessageProducer{TKey}"/>
        /// </summary>
        public string Name => _producer.Name;

        public ObjectMessageProducer(KafkaConfig config, ISerializingProducer<TKey, object> producer)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        public Task<Message<TKey, object>> ProduceAsync<TMessage>(string topic, TKey key, TMessage value)
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
