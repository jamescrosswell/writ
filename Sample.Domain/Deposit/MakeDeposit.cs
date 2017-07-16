using System;
using Newtonsoft.Json;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Deposit
{
    public class MakeDeposit : ICommand<Account, Guid>
    {
        public Guid Id { get; }
        public int Amount { get; }

        [JsonConstructor]
        public MakeDeposit(Guid id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}