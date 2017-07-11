using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public interface IObjectMessageProducer<TKey> : IDisposable
    {
        Task<Message<TKey, object>> ProduceAsync<TMessage>(string topic, TKey key, TMessage value);

        /// <summary>
        /// Gets the name of this producer instance (typically this would be unique).
        /// </summary>
        string Name { get; }
    }
}