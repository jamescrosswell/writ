using Microsoft.Extensions.Logging;
using Sample.Domain.Accounts;
using Sample.Domain.Payments;
using System;

namespace Sample.EventStore.Payments
{
    public class PaymentMadeHandler : SampleEventHandler<Account, Guid, PaymentMade>
    {
        public PaymentMadeHandler(ApplicationState applicationState, ILogger<SampleEventHandler<Account, Guid, PaymentMade>> logger)
            : base(applicationState, logger)
        {
        }

        public override void Handle(PaymentMade value)
        {
            var account = State.Accounts.FindById(value.Id);
            account.Balance -= value.Amount;
            State.Accounts.Update(account);
            Logger.LogInformation($"{account.AccountHolder} paid ${value.Amount} (transaction {value.Id})");
        }
    }
}