namespace Messaging
{
    public interface IEnvelopeHandler
    {
        /// <summary>
        /// Puts a message in an envelope, ready for sending
        /// </summary>
        /// <typeparam name="TMessage">The type of the message</typeparam>
        /// <param name="message">The message</param>
        /// <returns>A closed message envelope"/></returns>
        IMessageEnvelope<TMessage> Stuff<TMessage>(TMessage message);
        /// <summary>
        /// Extracts a previously enveloped message
        /// </summary>
        /// <typeparam name="TMessage">The message envelope type</typeparam>
        /// <param name="envelope">The envelope to be opened</param>
        /// <returns>The message that the envelope contains</returns>
        TMessage Open<TMessage>(IMessageEnvelope<TMessage> envelope);
    }
}