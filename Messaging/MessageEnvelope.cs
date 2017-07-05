using System;

namespace Messaging
{
    public class MessageEnvelope<TMessage> : IMessageEnvelope<TMessage>
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