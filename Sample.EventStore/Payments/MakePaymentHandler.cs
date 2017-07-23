using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Sample.Domain;
using Sample.Domain.Accounts;
using Sample.Domain.Payments;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Payments
{
    public class MakePaymentHandler : ObjectMessageHandler<string, MakePayment>
    {
        private readonly ILogger<MakePaymentHandler> _logger;
        private readonly IObjectMessageHandler<string, PaymentMade> _factHandler;
        private readonly ConventionalObjectMessageProducer<string, object> _producer;
        private readonly ApplicationState _applicationState;

        public MakePaymentHandler(
            ApplicationState applicationState, 
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, PaymentMade> factHandler,
            ILogger<MakePaymentHandler> logger
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factHandler = factHandler ?? throw new ArgumentNullException(nameof(factHandler));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, MakePayment value)
        {
            _logger.LogInformation($"Processing payment {value.Id}");

            // Check to make sure the account is valid
            var account = _applicationState.Accounts.FindById(value.Id);
            if (account == null)
                _producer.ProduceAsync(value.Failure<MakePayment, Account, Guid, PaymentMade>($"Invalid account {value.Id}"));
            else if (account.Balance - value.Amount < 0)
                _producer.ProduceAsync(value.Failure<MakePayment, Account, Guid, PaymentMade>($"Insufficient funds"));
            else
            {
                var fact = value.Succeess();
                _producer.ProduceAsync(fact);
                _factHandler.Handle(message, fact); // Applies the fact to the application state used to ensure command consistency
            }
        }
    }
}