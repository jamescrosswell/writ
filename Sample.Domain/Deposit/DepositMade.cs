using System;
using Newtonsoft.Json;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Deposit
{
    public class DepositMade : IEvent<Account, Guid>
    {
        public Guid Id { get; }
        public int Amount { get; }

        [JsonConstructor]
        public DepositMade(Guid id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}