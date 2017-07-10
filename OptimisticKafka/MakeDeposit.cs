using System;
using Newtonsoft.Json;

namespace OptimisticKafka
{
    class MakeDeposit : IEntity
    {
        public Guid Id { get; }
        public decimal Amount { get; }

        [JsonConstructor]
        public MakeDeposit(Guid id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        public MakeDeposit(decimal amount)
            : this(Guid.NewGuid(), amount)
        {
        }
    }
}