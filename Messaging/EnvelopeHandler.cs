using System;
using System.IO;

namespace Messaging
{
    public class EnvelopeHandler : IEnvelopeHandler
    {
        private readonly string _applicationName;
        private readonly CorrelationProvider _correlationProvider;

        public EnvelopeHandler(string applicationName, CorrelationProvider correlationProvider)
        {
            _applicationName = applicationName;
            _correlationProvider = correlationProvider ?? throw new ArgumentNullException(nameof(correlationProvider));
        }

        /// <inheritdoc cref="IEnvelopeHandler"/>
        public IMessageEnvelope<TMessage> Stuff<TMessage>(TMessage message)
        {            
            return new MessageEnvelope<TMessage>(
                _correlationProvider(),
                _applicationName,
                Environment.MachineName,
                message
                );
        }

        /// <inheritdoc cref="IEnvelopeHandler"/>
        public TMessage Open<TMessage>(IMessageEnvelope<TMessage> envelope)            
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            return envelope.Message;
        }
    }

    public delegate string CorrelationProvider();
}
