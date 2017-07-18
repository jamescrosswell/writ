using System;
using Newtonsoft.Json;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Deposits
{
    public class MakeDeposit : BaseCommand<Account, Guid>
    {
        public int Amount { get; }

        [JsonConstructor]
        public MakeDeposit(Guid id, int amount)
            : base(id)
        {
            Amount = amount;
        }

        public override IEvent<Account, Guid> Succeess() => new DepositMade(Id, Amount);
    }
}