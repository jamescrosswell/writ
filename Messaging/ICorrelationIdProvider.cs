namespace Messaging
{
    /// <summary>
    /// Provides a correlation id for the ongoing transaction
    /// </summary>
    /// <remarks>
    /// On a Web request, the TraceId would usually be appropriate. The 
    /// correlation id is used to group log messages that form part of 
    /// the same operation or transaction even when this operation is 
    /// carried out over multiple services/systems.
    /// </remarks>
    public interface ICorrelationIdProvider
    {
        /// <summary>
        /// Gets a correlation identifier.
        /// </summary>
        /// <returns>A string uniquely identifying the current operation</returns>
        string GetCurrentCorrelationId();
    }
}
