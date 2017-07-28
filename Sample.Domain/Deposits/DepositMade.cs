using Newtonsoft.Json;
using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Deposits
{
    public class DepositMade : BaseAccountEvent
    {
        public int Amount { get; }

        [JsonConstructor]
        public DepositMade(MessageOffset commandOffset, Guid id, int amount)
            : base(commandOffset, id)
        {
            Amount = amount;
        }
    }
}