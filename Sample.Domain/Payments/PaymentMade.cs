using Newtonsoft.Json;
using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Payments
{
    public class PaymentMade : BaseAccountEvent
    {
        public int Amount { get; }

        [JsonConstructor]
        public PaymentMade(MessageOffset commandOffset, Guid id, int amount)
            : base(commandOffset, id)
        {
            Amount = amount;
        }
    }
}