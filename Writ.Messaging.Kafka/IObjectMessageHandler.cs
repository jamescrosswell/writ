using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public interface IObjectMessageHandler<in TMessageValue>
    {
        void Handle(Message<string, object> message, TMessageValue value);
    }
}