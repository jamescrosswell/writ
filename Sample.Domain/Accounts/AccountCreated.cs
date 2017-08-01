using Newtonsoft.Json;
using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Accounts
{
    public class AccountCreated : BaseAccountEvent
    {
        public string AccountHolder { get; }

        [JsonConstructor]
        public AccountCreated(MessageOffset commandOffset, Guid id, string accountHolder)
            : base(commandOffset, id)
        {
            AccountHolder = accountHolder ?? throw new ArgumentNullException(nameof(accountHolder));
        }

        protected bool Equals(AccountCreated other)
        {
            return base.Equals(other) && string.Equals(AccountHolder, other.AccountHolder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AccountCreated)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (AccountHolder != null ? AccountHolder.GetHashCode() : 0);
            }
        }
    }
}