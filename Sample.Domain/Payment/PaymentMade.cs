using System;
using Newtonsoft.Json;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Payment
{
    public class PaymentMade : IEvent<Account, Guid>
    {
        public Guid Id { get; }
        public int Amount { get; }

        [JsonConstructor]
        public PaymentMade(Guid id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}