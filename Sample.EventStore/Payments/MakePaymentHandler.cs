using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.Domain.Accounts;
using Sample.Domain.Payments;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore.Payments
{
    public class MakePaymentHandler : SampleCommandHandler<Account, Guid, MakePayment, PaymentMade>
    {
        public MakePaymentHandler(
            ApplicationState applicationState, 
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, PaymentMade> factHandler,
            ILogger<MakePaymentHandler> logger
            )
            : base(applicationState, producer, factHandler, logger)
        {
        }

        protected override IEnumerable<CommandFailure<MakePayment, Account, Guid>> ValidateCommand(Message<string, object> message, MakePayment value)
        {
            // Check to make sure the account is valid
            var account = State.Accounts.FindById(value.Id);
            if (account == null)
                yield return value.Failure<MakePayment, Account, Guid>($"Invalid account {value.Id}");
            else if (account.Balance - value.Amount < 0)
                yield return value.Failure<MakePayment, Account, Guid>($"Insufficient funds");
        }

        protected override PaymentMade ProcessCommand(Message<string, object> message, MakePayment value)
        {
            Logger.LogInformation($"Processing payment {value.Id}");
            return new PaymentMade(message.TopicPartitionOffset, value.Id, value.Amount);
        }
    }
}