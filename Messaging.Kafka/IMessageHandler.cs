using Confluent.Kafka;

namespace Messaging.Kafka
{
    public interface IMessageHandler<TKey, TValue>
    {
        void Handle(Message<TKey, TValue> message);
    }
}