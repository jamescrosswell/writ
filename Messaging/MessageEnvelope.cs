using System;

namespace Messaging
{
    public class MessageEnvelope<TMessage> : IMessageEnvelope<TMessage>
    {
        public MessageEnvelope(string correlationId, string applicationName, string senderHostname, TMessage message)
        {
            CorrelationId = correlationId;
            ApplicationName = applicationName;
            SenderHostname = senderHostname;
            Message = message;
        }

        public string CorrelationId { get; }
        public string ApplicationName { get; }
        public string SenderHostname { get; }
        public TMessage Message { get; }

        public override string ToString()
        {
            return $"CorrelationId: {CorrelationId}, ApplicationName: {ApplicationName}, Hostname: {SenderHostname}, Message: {Message}";
        }
    }
}