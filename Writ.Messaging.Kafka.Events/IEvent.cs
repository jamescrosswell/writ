namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Marker interface for events
    /// </summary>
    public interface IEvent<TAggregateRoot, out TKey> : IAggregateConcern<TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        /// <summary>
        /// Identifies the command that generated this event. This can be useful for various reasons... 
        /// one of the main reasons we have it though is so that we can avoid processing commands more 
        /// than once (for example when restarting the command processor)
        /// </summary>
        MessageOffset CommandOffset { get; set; }
    }
}