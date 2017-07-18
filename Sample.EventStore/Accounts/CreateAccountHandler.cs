using Confluent.Kafka;
using Sample.Domain.Accounts;
using System;
using Microsoft.Extensions.Logging;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Accounts
{
    public class CreateAccountHandler : ObjectMessageHandler<string, CreateAccount>
    {
        private readonly ILogger<CreateAccountHandler> _logger;
        private readonly IObjectMessageHandler<string, AccountCreated> _factHandler;
        private readonly ConventionalObjectMessageProducer<string, object> _producer;
        private readonly ApplicationState _applicationState;

        public CreateAccountHandler(
            ApplicationState applicationState, 
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, AccountCreated> factHandler,
            ILogger<CreateAccountHandler> logger
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factHandler = factHandler ?? throw new ArgumentNullException(nameof(factHandler));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, CreateAccount value)
        {
            _logger.LogInformation($"Opening account {value.AccountHolder}");

            // Check to make sure the account doesn't already exist
            if (_applicationState.Accounts.Exists(x => x.Id == value.Id))
            {
                _producer.ProduceAsync(value.Failure($"Account {value.Id} already exist"));
                return;
            }

            var fact = value.Succeess();
            _producer.ProduceAsync(fact);
            _factHandler.Handle(message, (AccountCreated)fact); // Applies the fact to the application state used to ensure command consistency
        }
    }
}