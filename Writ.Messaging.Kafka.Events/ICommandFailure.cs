namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Interface for command failure messages. These get written to a special
    /// command failure topic back channel that would be read by client applications
    /// in addition to the standard events. Normally command failures would bubble
    /// up to the application UI and/or be written as errors to application logs.
    /// </summary>
    public interface ICommandFailure<out TCommand, TAggregateRoot, out TKey>: IAggregateConcern<TKey>
        where TCommand : ICommand<TAggregateRoot, TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        TCommand Command { get; }
        string Reason { get; }
    }
}