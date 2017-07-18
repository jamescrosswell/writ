using Confluent.Kafka;
using Sample.Domain.Deposits;
using System;
using Microsoft.Extensions.Logging;
using Writ.Messaging.Kafka;

namespace Sample.EventStore.Deposits
{
    public class DepositMadeHandler : ObjectMessageHandler<string, DepositMade>
    {
        private readonly ILogger<DepositMadeHandler> _logger;
        private readonly ApplicationState _applicationState;

        public DepositMadeHandler(ApplicationState applicationState, ILogger<DepositMadeHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _applicationState = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public override void Handle(Message<string, object> message, DepositMade value)
        {
            var account = _applicationState.Accounts.FindById(value.Id);
            account.Balance += value.Amount;
            _applicationState.Accounts.Update(account);

            _logger.LogInformation($"{account.AccountHolder} deposited ${value.Amount} (transaction {value.Id})");
        }
    }
}