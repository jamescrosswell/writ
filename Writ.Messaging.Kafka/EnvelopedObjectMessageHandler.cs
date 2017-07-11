using Confluent.Kafka;
using System;

namespace Writ.Messaging.Kafka
{
    public class EnvelopedObjectMessageHandler<TKey, TMessageValue> : IMessageHandler<TKey, object>
        where TMessageValue : class
    {
        private readonly IObjectMessageHandler<TKey, TMessageValue> _wrappedHandler;

        public EnvelopedObjectMessageHandler(IObjectMessageHandler<TKey, TMessageValue> wrappedHandler)
        {
            _wrappedHandler = wrappedHandler ?? throw new ArgumentNullException(nameof(wrappedHandler));
        }

        void IMessageHandler<TKey, object>.Handle(Message<TKey, object> message)
        {
            if (!(message?.Value is IMessageEnvelope<TMessageValue> envelope)) return;
            _wrappedHandler.Handle(message, envelope.Message);
        }
    }
}