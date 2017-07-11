using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public interface IObjectMessageHandler<TKey, in TMessageValue>
    {
        void Handle(Message<TKey, object> message, TMessageValue value);
    }
}