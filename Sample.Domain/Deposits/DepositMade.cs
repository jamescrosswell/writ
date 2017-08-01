using Newtonsoft.Json;
using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Deposits
{
    public class DepositMade : BaseAccountEvent
    {
        public int Amount { get; }

        [JsonConstructor]
        public DepositMade(MessageOffset commandOffset, Guid id, int amount)
            : base(commandOffset, id)
        {
            Amount = amount;
        }

        protected bool Equals(DepositMade other)
        {
            return base.Equals(other) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DepositMade)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Amount;
            }
        }
    }
}