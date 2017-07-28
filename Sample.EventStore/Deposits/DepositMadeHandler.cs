using Sample.Domain.Deposits;
using System;
using Microsoft.Extensions.Logging;
using Sample.Domain.Accounts;

namespace Sample.EventStore.Deposits
{
    public class DepositMadeHandler : SampleEventHandler<Account, Guid, DepositMade>
    {
        public DepositMadeHandler(ApplicationState applicationState, ILogger<DepositMadeHandler> logger) 
            : base(applicationState, logger)
        {
        }

        public override void Handle(DepositMade value)
        {
            var account = State.Accounts.FindById(value.Id);
            account.Balance += value.Amount;
            State.Accounts.Update(account);
            Logger.LogInformation($"{account.AccountHolder} deposited ${value.Amount} (transaction {value.Id})");
        }
    }
}