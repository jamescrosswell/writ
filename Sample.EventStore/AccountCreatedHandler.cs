using System;
using Confluent.Kafka;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;

namespace Sample.EventStore
{
    public class AccountCreatedHandler : ObjectMessageHandler<string, AccountCreated>
    {
        private readonly ApplicationState _applicationState;

        public AccountCreatedHandler(ApplicationState applicationState)
        {
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
        }
    }
}