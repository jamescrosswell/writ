using System;

namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// Interface for command failure messages. These get written to a special
    /// command failure topic back channel that would be read by client applications
    /// in addition to the standard events. Normally command failures would bubble
    /// up to the application UI and/or be written as errors to application logs.
    /// </summary>
    public interface ICommandFailure<TAggregateRoot, out TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        ICommand<TAggregateRoot, TKey> Command { get; }
        string Reason { get; }
    }

    public class CommandFailure<TAggregateRoot, TKey> : ICommandFailure<TAggregateRoot, TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        public ICommand<TAggregateRoot, TKey> Command { get; }
        public string Reason { get; }

        public CommandFailure(ICommand<TAggregateRoot, TKey> command, string reason)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}