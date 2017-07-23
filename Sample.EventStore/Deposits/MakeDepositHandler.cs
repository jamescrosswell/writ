using Confluent.Kafka;
using Sample.Domain.Deposits;
using System;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Deposits
{
    public class MakeDepositHandler : ObjectMessageHandler<string, MakeDeposit>
    {
        private readonly ILogger<MakeDepositHandler> _logger;
        private readonly IObjectMessageHandler<string, DepositMade> _factHandler;
        private readonly ConventionalObjectMessageProducer<string, object> _producer;
        private readonly ApplicationState _applicationState;

        public MakeDepositHandler(
            ApplicationState applicationState, 
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, DepositMade> factHandler,
            ILogger<MakeDepositHandler> logger
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factHandler = factHandler ?? throw new ArgumentNullException(nameof(factHandler));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, MakeDeposit value)
        {
            _logger.LogInformation($"Processing deposit {value.Id}");

            // Check to make sure the account doesn't already exist
            var account = _applicationState.Accounts.FindById(value.Id);
            if (account == null)
            {
                _producer.ProduceAsync(value.Failure<MakeDeposit, Account, Guid, DepositMade>($"Invalid account {value.Id}"));
                return;
            }

            var fact = value.Succeess();
            _producer.ProduceAsync(fact);
            _factHandler.Handle(message, fact); // Applies the fact to the application state used to ensure command consistency
        }
    }
}