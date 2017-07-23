using System;

namespace Writ.Messaging.Kafka.Events
{
    public class CommandFailure<TCommand, TAggregateRoot, TKey> : ICommandFailure<TCommand, TAggregateRoot, TKey>
        where TCommand : ICommand<TAggregateRoot, TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        public TKey Id => Command.Id;
        public TCommand Command { get; }
        public string Reason { get; }

        public CommandFailure(TCommand command, string reason)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            Command = command;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }
    }
}