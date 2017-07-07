using Confluent.Kafka;

namespace Messaging.Kafka
{
    public interface IObjectMessageHandler<in TMessageValue>
    {
        void Handle(Message<string, object> message, TMessageValue value);
    }
}