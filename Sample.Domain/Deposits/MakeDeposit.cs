using Newtonsoft.Json;
using Sample.Domain.Accounts;
using System;

namespace Sample.Domain.Deposits
{
    public class MakeDeposit : BaseCommand<Account, Guid>
    {
        public int Amount { get; }

        [JsonConstructor]
        public MakeDeposit(Guid id, int amount)
            : base(id)
        {
            Amount = amount;
        }

        protected bool Equals(MakeDeposit other)
        {
            return base.Equals(other) && Amount == other.Amount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MakeDeposit)obj);
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