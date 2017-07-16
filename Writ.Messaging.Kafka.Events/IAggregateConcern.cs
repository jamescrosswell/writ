namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Base interface for all messages concerning the same aggregate,
    /// all of which should share the same key (and thus key type)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IAggregateConcern<out TKey>
    {
        TKey Id { get; }
    }
}