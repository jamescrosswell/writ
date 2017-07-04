using System;

namespace OptimisticKafka
{
    class MakeDeposit : Entity
    {
        internal MakeDeposit()
        {
            
        }

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