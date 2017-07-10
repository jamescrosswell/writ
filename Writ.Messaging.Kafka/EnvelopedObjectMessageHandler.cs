using System;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Writ.Messaging.Kafka
{
    public class EnvelopedObjectMessageHandler<TMessageValue> : IMessageHandler<string, object>
        where TMessageValue : class
    {
        private readonly IObjectMessageHandler<TMessageValue> _wrappedHandler;

        public EnvelopedObjectMessageHandler(IObjectMessageHandler<TMessageValue> wrappedHandler)
        {
            _wrappedHandler = wrappedHandler ?? throw new ArgumentNullException(nameof(wrappedHandler));
        }

        void IMessageHandler<string, object>.Handle(Message<string, object> message)
        {
            if (!(message?.Value is IMessageEnvelope<TMessageValue> envelope)) return;
            _wrappedHandler.Handle(message, envelope.Message);
        }
    }

    public static class EnvelopedObjectMessageHandlerExtensions
    {
        public static void AddEnvelopedHandler<TMessageHandler, TMessageValue>(this IServiceCollection services, TMessageHandler handler)
            where TMessageValue : class
            where TMessageHandler : ObjectMessageHandler<TMessageValue>
        {
            services.AddTransient<IObjectMessageHandler<TMessageValue>, TMessageHandler>();
            services.AddTransient<IMessageHandler<string, object>, EnvelopedObjectMessageHandler<TMessageValue>>();

        }
    }
}