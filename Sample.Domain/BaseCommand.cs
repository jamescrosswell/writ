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

        public CommandFailure<TAggregateRoot, TKey> Failure(string reason)
        {
            return new CommandFailure<TAggregateRoot, TKey>(this, reason);
        }

        public abstract IEvent<TAggregateRoot, TKey> Succeess();
    }
}