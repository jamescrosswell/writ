using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka;

namespace Sample.EventStore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEventStore(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationState>();
            services.AddHandler<string, AccountCreated, AccountCreatedHandler>();
        }
    }
}
