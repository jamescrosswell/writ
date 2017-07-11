using System;
using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public abstract class ObjectMessageHandler<TKey, TMessageValue> : IMessageHandler<TKey, object>, IObjectMessageHandler<TKey, TMessageValue> 
        where TMessageValue : class
    {
        public void Handle(Message<TKey, object> message)
        {
            if (message?.Value is TMessageValue value)
                Handle(message, value);
        }

        public abstract void Handle(Message<TKey, object> message, TMessageValue value);
    }
}