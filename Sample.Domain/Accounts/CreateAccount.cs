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
    }
}