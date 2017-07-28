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
    }
}