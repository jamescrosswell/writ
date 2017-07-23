using Writ.Messaging.Kafka.Events;

namespace Sample.Domain
{
    public abstract class BaseCommand<TAggregateRoot, TKey, TSuccess> : ICommand<TAggregateRoot, TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
        where TSuccess : IEvent<TAggregateRoot, TKey>
    {
        public TKey Id { get; }

        protected BaseCommand(TKey id)
        {
            Id = id;
        }

        public abstract TSuccess Succeess();
    }

    public static class BaseCommandExtensions
    {
        public static CommandFailure<TCommand, TAggregateRoot, TKey> Failure<TCommand, TAggregateRoot, TKey, TSuccess>(
            this TCommand command, string reason
            )
            where TCommand: BaseCommand<TAggregateRoot, TKey, TSuccess>
            where TAggregateRoot : IAggregateRoot<TKey>
            where TSuccess : IEvent<TAggregateRoot, TKey>
        {
            return new CommandFailure<TCommand, TAggregateRoot, TKey>(command, reason);
        }
    }
}