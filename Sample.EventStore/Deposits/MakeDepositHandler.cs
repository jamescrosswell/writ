using Confluent.Kafka;
using Sample.Domain.Deposits;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore.Deposits
{
    public class MakeDepositHandler : SampleCommandHandler<Account, Guid, MakeDeposit, DepositMade>
    {
        public MakeDepositHandler(
            ApplicationState applicationState, 
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, DepositMade> factHandler,
            ILogger<MakeDepositHandler> logger
            )
            : base(applicationState, producer, factHandler, logger)
        {
        }

        protected override IEnumerable<CommandFailure<MakeDeposit, Account, Guid>> ValidateCommand(Message<string, object> message, MakeDeposit value)
        {
            // Check to make sure the account exists
            var account = State.Accounts.FindById(value.Id);
            if (account == null)
                yield return value.Failure<MakeDeposit, Account, Guid>($"Invalid account {value.Id}");
            if (value.Amount <= 0)
                yield return value.Failure<MakeDeposit, Account, Guid>($"Invalid deposit amount {value.Amount}");
        }

        protected override DepositMade ProcessCommand(Message<string, object> message, MakeDeposit value)
        {
            Logger.LogInformation($"Processing deposit {value.Id}");
            return new DepositMade(message.TopicPartitionOffset, value.Id, value.Amount);
        }        
    }
}