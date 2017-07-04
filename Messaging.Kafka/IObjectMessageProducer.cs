using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Messaging.Kafka
{
    public interface IObjectMessageProducer: IDisposable
    {
        Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value);

        /// <summary>
        /// Gets the name of this producer instance (typically this would be unique).
        /// </summary>
        string Name { get; }
    }
}