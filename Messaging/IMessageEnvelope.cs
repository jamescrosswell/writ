using System;

namespace Messaging
{
    /// <summary>
    /// An envelope to <typeparam name="TMessage"></typeparam>
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    public interface IMessageEnvelope<out TMessage>
    {
        /// <summary>
        /// Gets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        string CorrelationId { get; }
        /// <summary>
        /// Gets the name of the application which created the message
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        string ApplicationName { get; }
        /// <summary>
        /// Gets the host name of the machine which created the message
        /// </summary>
        /// <value>
        /// The host name.
        /// </value>
        string SenderHostname { get; }
        /// <summary>
        /// The enveloped message 
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        TMessage Message { get; }
    }
}
