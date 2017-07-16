using Confluent.Kafka;
using System;
using Confluent.Kafka.Serialization;

namespace Writ.Messaging.Kafka
{
    /// <summary>
    /// This consumer has it's own OnMessage event handler that dispatches messages 
    /// to any appropriate message handlers that have been registered as services.
    /// </summary>
    public class DispatchingConsumer<TKey, TValue> : Consumer<TKey, TValue>
    {
        private readonly IServiceProvider _serviceProvider;

        public DispatchingConsumer(
            IServiceProvider serviceProvider, 
            KafkaConfig config, 
            IDeserializer<TKey> keyDeserializer, 
            IDeserializer<TValue> valueDeserializer
            ) : base(config, keyDeserializer, valueDeserializer)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            OnMessage += DispatchMessage;
        }

        private void DispatchMessage(object sender, Message<TKey, TValue> message)
        {
            if (message.Value == null)
                return;
            var handlerType = typeof(IMessageHandler<,>).MakeGenericType(typeof(TKey), message.Value.GetType());
            var handler = _serviceProvider.GetService(handlerType);
            if (handler is IMessageHandler<TKey, TValue> messageHandler)
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
    }
}
