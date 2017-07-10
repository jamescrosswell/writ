using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public interface IMessageHandler<TKey, TValue>
    {
        void Handle(Message<TKey, TValue> message);
    }
}