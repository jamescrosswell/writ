using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Accounts
{
    public class AccountCreatedHandler : ObjectMessageHandler<string, AccountCreated>
    {
        private readonly ILogger<AccountCreatedHandler> _logger;
        private readonly ApplicationState _applicationState;

        public AccountCreatedHandler(ApplicationState applicationState, ILogger<AccountCreatedHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, AccountCreated value)
        {
            var account = new Account
            {
                Id = value.Id,
                AccountHolder = value.AccountHolder,
                Balance = 0
            };
            _applicationState.Accounts.Insert(account);

            _logger.LogInformation($"{account.AccountHolder} account opened");

        }
    }
}