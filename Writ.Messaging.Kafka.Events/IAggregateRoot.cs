namespace Writ.Messaging.Kafka.Events
{
    public interface IAggregateRoot<out TKey>: IAggregateConcern<TKey>
    {
    }
}