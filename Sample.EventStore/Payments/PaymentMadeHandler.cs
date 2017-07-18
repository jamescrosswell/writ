using Confluent.Kafka;
using Sample.Domain.Payments;
using System;
using Microsoft.Extensions.Logging;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Payments
{
    public class PaymentMadeHandler : ObjectMessageHandler<string, PaymentMade>
    {
        private readonly ILogger<PaymentMadeHandler> _logger;
        private readonly ApplicationState _applicationState;

        public PaymentMadeHandler(ApplicationState applicationState, ILogger<PaymentMadeHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, PaymentMade value)
        {
            var account = _applicationState.Accounts.FindById(value.Id);
            account.Balance -= value.Amount;
            _applicationState.Accounts.Update(account);

            _logger.LogInformation($"{account.AccountHolder} paid ${value.Amount} (transaction {value.Id})");
        }
    }
}