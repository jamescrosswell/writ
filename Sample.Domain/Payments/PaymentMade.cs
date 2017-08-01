using Newtonsoft.Json;
using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Payments
{
    public class PaymentMade : BaseAccountEvent
    {
        public int Amount { get; }

        [JsonConstructor]
        public PaymentMade(MessageOffset commandOffset, Guid id, int amount)
            : base(commandOffset, id)
        {
            Amount = amount;
        }

        protected bool Equals(PaymentMade other)
        {
            return base.Equals(other) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PaymentMade)obj);
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