using System.Collections.Generic;
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

        protected bool Equals(BaseCommand<TAggregateRoot, TKey> other)
        {
            return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseCommand<TAggregateRoot, TKey>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TKey>.Default.GetHashCode(Id);
        }
    }

    public static class BaseCommandExtensions
    {
        public static CommandFailure<TCommand, TAggregateRoot, TKey> Failure<TCommand, TAggregateRoot, TKey>(
            this TCommand command, string reason
            )
            where TCommand: BaseCommand<TAggregateRoot, TKey>
            where TAggregateRoot : IAggregateRoot<TKey>
        {
            return new CommandFailure<TCommand, TAggregateRoot, TKey>(command, reason);
        }
    }
}