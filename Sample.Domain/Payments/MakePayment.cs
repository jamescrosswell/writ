using Newtonsoft.Json;
using Sample.Domain.Accounts;
using System;

namespace Sample.Domain.Payments
{
    public class MakePayment : BaseCommand<Account, Guid, PaymentMade>
    {
        public int Amount { get; }

        [JsonConstructor]
        public MakePayment(Guid id, int amount)
            : base(id)
        {
            Amount = amount;
        }

        public override PaymentMade Succeess() => new PaymentMade(Id, Amount);
    }
}