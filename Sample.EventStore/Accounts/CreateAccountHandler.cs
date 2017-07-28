using Confluent.Kafka;
using Sample.Domain.Accounts;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore.Accounts
{
    public class CreateAccountHandler : SampleCommandHandler<Account, Guid, CreateAccount, AccountCreated>
    {

        public CreateAccountHandler(
            ApplicationState applicationState,
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, AccountCreated> factHandler,
            ILogger<CreateAccountHandler> logger
        )
            : base(applicationState, producer, factHandler, logger)
        {
        }

        protected override IEnumerable<CommandFailure<CreateAccount, Account, Guid>> ValidateCommand(Message<string, object> message, CreateAccount value)
        {
            // Check to make sure the account doesn't already exist
            if (State.Accounts.Exists(x => x.Id == value.Id))
                yield return value.Failure<CreateAccount, Account, Guid>($"Account {value.Id} already exist");
        }

        protected override AccountCreated ProcessCommand(Message<string, object> message, CreateAccount value)
        {
            Logger.LogInformation($"Opening account {value.AccountHolder}");
            return new AccountCreated(message.TopicPartitionOffset, value.Id, value.AccountHolder);
        }
    }
}