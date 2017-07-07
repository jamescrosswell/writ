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
            this.Id = id;
            this.Amount = amount;
        }

        public MakeDeposit(decimal amount)
            : this(Guid.NewGuid(), amount)
        {
        }
    }
}