using System;
using Confluent.Kafka;

namespace Messaging.Kafka
{
    public class ProducerProxy
    {
        private readonly Producer<string, object> _producer;

        public ProducerProxy(Producer<string, object> producer)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        /// <summary>
        /// <inheritdoc cref="Producer{TKey, TValue}"/>
        /// </summary>
        public string Name => _producer.Name;

        /// <summary>
        /// <inheritdoc cref="Producer{TKey, TValue}"/>
        /// </summary>
        public int Flush(int millisecondsTimeout)
        {
            return _producer.Flush(millisecondsTimeout);
        }

        /// <summary>
        /// <inheritdoc cref="Producer{TKey, TValue}"/>
        /// </summary>
        public int Flush(TimeSpan timeout)
        {
            return _producer.Flush(timeout);
        }
    }
}