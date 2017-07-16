using System;
using Newtonsoft.Json;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Payment
{
    public class MakePayment : ICommand<Account, Guid>
    {
        public Guid Id { get; }
        public int Amount { get; }

        [JsonConstructor]
        public MakePayment(Guid id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}