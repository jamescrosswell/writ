using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Messaging.Kafka
{
    public interface IObjectMessageProducer: IDisposable
    {
        ProducerProxy Internal { get; }

        Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value);
    }
}