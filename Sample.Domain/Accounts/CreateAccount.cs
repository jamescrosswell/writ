using Newtonsoft.Json;
using System;

namespace Sample.Domain.Accounts
{
    public class CreateAccount : BaseCommand<Account, Guid>
    {
        public string AccountHolder { get; }

        [JsonConstructor]
        public CreateAccount(Guid id, string accountHolder)
            : base(id)
        {
            AccountHolder = accountHolder ?? throw new ArgumentNullException(nameof(accountHolder));
        }

        protected bool Equals(CreateAccount other)
        {
            return base.Equals(other) && string.Equals(AccountHolder, other.AccountHolder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CreateAccount)obj);
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