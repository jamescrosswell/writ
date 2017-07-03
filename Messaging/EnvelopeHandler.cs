using System;

namespace Messaging
{
    public class EnvelopeHandler : IEnvelopeHandler
    {
        private readonly string _applicationName;
        private readonly ICorrelationIdProvider _correlationProvider;

        public EnvelopeHandler(string applicationName, ICorrelationIdProvider correlationProvider)
        {
            _applicationName = applicationName;
            _correlationProvider = correlationProvider ?? throw new ArgumentNullException(nameof(correlationProvider));
        }

        /// <inheritdoc cref="IEnvelopeHandler"/>
        public IMessageEnvelope<TMessage> Stuff<TMessage>(TMessage message)
        {            
            return new MessageEnvelope<TMessage>(
                _correlationProvider.GetCurrentCorrelationId(),
                _applicationName,
                Environment.MachineName,
                DateTimeOffset.UtcNow,
                message
                );
        }

        /// <inheritdoc cref="IEnvelopeHandler"/>
        public TMessage Open<TMessage>(IMessageEnvelope<TMessage> envelope)
        {
            return envelope.Message;
        }

        private class MessageEnvelope<TMessage> : IMessageEnvelope<TMessage>
        {
            public MessageEnvelope(string correlationId, string applicationName, string senderHostname, DateTimeOffset createdTime, TMessage payload)
            {
                CorrelationId = correlationId;
                ApplicationName = applicationName;
                SenderHostname = senderHostname;
                CreatedTime = createdTime;
                Message = payload;
            }

            public string CorrelationId { get; }
            public string ApplicationName { get; }
            public string SenderHostname { get; }
            public DateTimeOffset CreatedTime { get; }
            public TMessage Message { get; }

            public override string ToString()
            {
                return $"CorrelationId: {CorrelationId}, ApplicationName: {ApplicationName}, Hostname: {SenderHostname}, CreatedTime: {CreatedTime}, Message: {Message}";
            }
        }
    }
}
