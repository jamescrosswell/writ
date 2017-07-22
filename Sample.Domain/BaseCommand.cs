using Writ.Messaging.Kafka.Events;

namespace Sample.Domain
{
    public abstract class BaseCommand<TAggregateRoot, TKey> : ICommand<TAggregateRoot, TKey>
        where TAggregateRoot : IAggregateRoot<TKey>
    {
        public TKey Id { get; }

        protected BaseCommand(TKey id)
        {
            Id = id;
        }

        public abstract IEvent<TAggregateRoot, TKey> Succeess();
    }

    public static class BaseCommandExtensions
    {
        public static CommandFailure<TCommand, TAggregateRoot, TKey> Failure<TCommand, TAggregateRoot, TKey>(this TCommand command, string reason)
            where TCommand: BaseCommand<TAggregateRoot, TKey>
            where TAggregateRoot : IAggregateRoot<TKey>
        {
            return new CommandFailure<TCommand, TAggregateRoot, TKey>(command, reason);
        }
    }
}