namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Marker interface for commands, which get written to command topic of the 
    /// aggregate root
    /// </summary>
    public interface ICommand<TAggregateRoot, out TKey>: IAggregateConcern<TKey>
        where TAggregateRoot: IAggregateRoot<TKey>
    { }
}