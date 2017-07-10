using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    /// <summary>
    /// This decorator allows us to wrap kafka object messages in an envelope that makes it easier to track
    /// the transit of these messages through intermediate nodes and deserialize them.
    /// </summary>
    public class EnvelopedObjectMessageProducer : IObjectMessageProducer
    {
        private readonly IEnvelopeHandler _handler;
        private readonly IObjectMessageProducer _wrappedProducer;

        /// <summary>
        /// Creates an EnvelopedObjectMessageProducer
        /// </summary>
        /// <param name="producer">An object message producer to wrap</param>
        /// <param name="handler"></param>
        public EnvelopedObjectMessageProducer(IObjectMessageProducer producer, IEnvelopeHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _wrappedProducer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        public Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value)
        {
            var envelopedMessage = _handler.Stuff(value);
            return _wrappedProducer.ProduceAsync(topic, key, envelopedMessage);
        }

        public void Dispose()
        {
            _wrappedProducer?.Dispose();
        }

        public string Name => _wrappedProducer.Name;
    }
}