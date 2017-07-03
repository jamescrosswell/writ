using System;

namespace OptimisticKafka
{
    class MakeDeposit : Entity
    {
        public MakeDeposit(Guid id, decimal amount)
            : base(id)
        {
            this.Amount = amount;
        }

        public MakeDeposit(decimal amount)
            : this(Guid.NewGuid(), amount)
        {
        }

        public decimal Amount { get; }
    }
}