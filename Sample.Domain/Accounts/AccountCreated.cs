using System;
using Newtonsoft.Json;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Accounts
{
    public class AccountCreated : IEvent<Account, Guid>
    {
        public Guid Id { get; }
        public string AccountHolder { get; }

        [JsonConstructor]
        public AccountCreated(Guid id, string accountHolder)
        {
            Id = id;
            AccountHolder = accountHolder ?? throw new ArgumentNullException(nameof(accountHolder));
        }
    }
}