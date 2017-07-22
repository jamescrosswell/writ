using System;
using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Accounts;
using Sample.Domain.Deposits;
using Sample.Domain.Payments;
using Writ.Messaging;
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

        /// <summary>
        /// Normally we'd use a schema registry somewhere and schemas would have namespaces and names,
        /// which would be language/application agnostic. For our simple sample app however our producer
        /// and consumer share knowledge of the types being serialized and we don't have multiple 
        /// different message/schema versions to deal with, so we're using a simple convention of the 
        /// type name and version 1 for each of the messages that we need to deserialize
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ISchemaTypeMap AddSampleSchemaConventions(this IServiceCollection services)
        {
            var typeMap = new SchemaTypeMap();

            void AddMessageSchema<TMessage>()
            {
                var typeName = typeof(TMessage).Name;
                typeMap.RegisterTypeSchema<TMessage>(typeName);
                typeMap.RegisterTypeSchema<MessageEnvelope<TMessage>>($"{typeName}Envelope");
            }

            void AddCommandSchema<TMessage>()
            {
                AddMessageSchema<TMessage>();
                typeMap.RegisterTypeSchema<CommandFailure<CreateAccount, Account, Guid>>($"{typeof(TMessage).Name}CommandFailure");
            }

            AddCommandSchema<CreateAccount>();
            AddCommandSchema<MakeDeposit>();
            AddCommandSchema<MakePayment>();

            AddMessageSchema<AccountCreated>();
            AddMessageSchema<DepositMade>();
            AddMessageSchema<PaymentMade>();

            services.AddSingleton<ISchemaTypeMap>(typeMap);

            return typeMap;
        }        
    }
}
