using System;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Messaging.Kafka
{
    /// <summary>
    /// This decorator allows us to wrap kafka object messages in an envelope that makes it easier to track
    /// the transit of these messages through intermediate nodes and deserialize them.
    /// </summary>
    public class EnvelopedObjectMessageConsumer : IObjectMessageConsumer
    {
        private readonly IEnvelopeHandler _handler;
        private readonly IObjectMessageConsumer _wrappedConsumer;

        /// <summary>
        ///  <inheritdoc cref="IObjectMessageConsumer"/>
        /// </summary>
        public string Name => _wrappedConsumer.Name;

        /// <summary>
        /// Creates an EnvelopedObjectMessageConsumer
        /// </summary>
        /// <param name="consumer">An object message consumer to wrap</param>
        /// <param name="handler">A hander that can open message envelopes</param>
        public EnvelopedObjectMessageConsumer(IObjectMessageConsumer consumer, IEnvelopeHandler handler)
        {
            _wrappedConsumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        bool IObjectMessageConsumer.Consume<TMessage>(out Message<string, TMessage> message, TimeSpan timeout)
        {
            var result = _wrappedConsumer.Consume(out Message<string, MessageEnvelope<TMessage>> fetched, timeout);
            message = fetched?.Repackage(_handler.Open(fetched.Value));
            return result;
        }

        public void Dispose()
        {
            _wrappedConsumer?.Dispose();
        }
    }
}