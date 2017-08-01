using Newtonsoft.Json;
using Sample.Domain.Accounts;
using System;

namespace Sample.Domain.Payments
{
    public class MakePayment : BaseCommand<Account, Guid>
    {
        public int Amount { get; }

        [JsonConstructor]
        public MakePayment(Guid id, int amount)
            : base(id)
        {
            Amount = amount;
        }

        protected bool Equals(MakePayment other)
        {
            return base.Equals(other) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MakePayment)obj);
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