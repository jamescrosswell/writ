using System;
using Confluent.Kafka;

namespace Messaging.Kafka
{
    public interface IObjectMessageConsumer: IDisposable
    {
        /// <summary>
        /// A unique name assigned to the underlying kafka consumer (appropriate for logging/debugging).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// <inheritdoc cref="Consumer{TKey,TValue}"/>
        /// </summary>
        bool Consume<TMessage>(out Message<string, TMessage> message, TimeSpan timeout) where TMessage: class;

        void Subscribe(string topic);
    }
}