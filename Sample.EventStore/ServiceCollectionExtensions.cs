using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Accounts;
using Sample.Domain.Deposits;
using Sample.Domain.Payments;
using Sample.EventStore.Accounts;
using Sample.EventStore.Deposits;
using Sample.EventStore.Payments;
using Writ.Messaging.Kafka;

namespace Sample.EventStore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationState(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationState>();
        }

        public static void AddCommandHandlers(this IServiceCollection services)
        {
            services.AddHandler<string, CreateAccount, CreateAccountHandler>();
            services.AddHandler<string, MakeDeposit, MakeDepositHandler>();
            services.AddHandler<string, MakePayment, MakePaymentHandler>();
        }

        public static void AddEventHandlers(this IServiceCollection services)
        {
            services.AddHandler<string, AccountCreated, AccountCreatedHandler>();
            services.AddHandler<string, DepositMade, DepositMadeHandler>();
            services.AddHandler<string, PaymentMade, PaymentMadeHandler>();
        }
    }
}
