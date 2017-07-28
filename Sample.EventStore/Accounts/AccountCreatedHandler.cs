using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Accounts
{
    public class AccountCreatedHandler : SampleEventHandler<Account, Guid, AccountCreated>
    {
        public AccountCreatedHandler(ApplicationState applicationState, ILogger<SampleEventHandler<Account, Guid, AccountCreated>> logger) 
            : base(applicationState, logger)
        {
        }

        public override void Handle(AccountCreated value)
        {
            var account = new Account
            {
                Id = value.Id,
                AccountHolder = value.AccountHolder,
                Balance = 0
            };
            State.Accounts.Insert(account);
            Logger.LogInformation($"{account.AccountHolder} account opened");
        }

    }
}