using System;
using System.Collections.Generic;
using Confluent.Kafka;

namespace Messaging.Kafka
{
    public class ObjectMessageDispatcher : IDisposable
    {
        private Consumer<string, object> _consumer;
        private readonly IEnumerable<IMessageHandler<string, object>> _messageHandlers;

        public ObjectMessageDispatcher(IEnumerable<IMessageHandler<string, object>> messageHandlers)
        {
            _messageHandlers = messageHandlers ?? throw new ArgumentNullException(nameof(messageHandlers));
        }

        public void DispatchMessagesFor(Consumer<string, object> consumer)
        {
            if (_consumer != null)
                throw new InvalidOperationException("This dispatcher is already initialized");
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _consumer.OnMessage += OnConsumerMessage; 
        }

        private void OnConsumerMessage(object sender, Message<string, object> message)
        {
            foreach (var handler in _messageHandlers)
            {
                handler.Handle(message);
            }
        }

        public void Dispose()
        {
            _consumer.OnMessage -= OnConsumerMessage;
        }
    }
}
