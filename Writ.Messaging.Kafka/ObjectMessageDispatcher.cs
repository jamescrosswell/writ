using Confluent.Kafka;
using System;
using System.Collections.Generic;

namespace Writ.Messaging.Kafka
{
    public class ObjectMessageDispatcher<TKey> : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Consumer<TKey, object> _consumer;
        private readonly IEnumerable<IMessageHandler<TKey, object>> _messageHandlers;

        public ObjectMessageDispatcher(IServiceProvider serviceProvider, IEnumerable<IMessageHandler<TKey, object>> messageHandlers)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _messageHandlers = messageHandlers ?? throw new ArgumentNullException(nameof(messageHandlers));
        }

        public void DispatchMessagesFor(Consumer<TKey, object> consumer)
        {
            if (_consumer != null)
                throw new InvalidOperationException("This dispatcher is already initialized");
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _consumer.OnMessage += OnConsumerMessage; 
        }

        private void OnConsumerMessage(object sender, Message<TKey, object> message)
        {
            if (message.Value == null)
                return;
            var handlerType = typeof(IMessageHandler<,>).MakeGenericType(typeof(TKey), message.Value.GetType());
            var handler = _serviceProvider.GetService(handlerType);
            if (handler is IMessageHandler<TKey, object> messageHandler)
            {
                messageHandler.Handle(message);
            }

            // TODO: For some reason the service provider throws an exception when we try to get an array of handlers... could be a bug in .NET
            //var handlers = _serviceProvider.GetServices(handlerType);
            //foreach (IMessageHandler<TKey, object> handler in handlers)
            //{
            //    handler.Handle(message);
            //}

        }

        public void Dispose()
        {
            _consumer.OnMessage -= OnConsumerMessage;
        }
    }
}
