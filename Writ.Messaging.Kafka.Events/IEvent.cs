namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Marker interface for events
    /// </summary>
    public interface IEvent<TAggregateRoot, out TKey>: IAggregateConcern<TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    { }
}