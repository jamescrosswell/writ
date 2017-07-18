using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static AggregateRootRegistry AddSampleAggregates(this IServiceCollection services)
        {
            var aggregates = new AggregateRootRegistry();
            aggregates.RegisterAggregate<Account>();
            services.AddSingleton(aggregates);
            return aggregates;
        }
    }
}
