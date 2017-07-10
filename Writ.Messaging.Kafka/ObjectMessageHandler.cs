using System;
using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public abstract class ObjectMessageHandler<TMessageValue> : IMessageHandler<string, object>, IObjectMessageHandler<TMessageValue> 
        where TMessageValue : class
    {
        void IMessageHandler<string, object>.Handle(Message<string, object> message)
        {
            if (message?.Value is TMessageValue value)
                Handle(message, value);
        }

        public abstract void Handle(Message<string, object> message, TMessageValue value);
    }
}