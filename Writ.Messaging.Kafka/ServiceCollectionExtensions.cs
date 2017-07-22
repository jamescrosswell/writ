using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Writ.Messaging.Kafka
{
    /// <summary>
    /// Helper extensions to register Writ messaging services for kafka
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static void AddWrit<TKey, TEntityBase>(this IServiceCollection services, WritKafkaServices<TKey, TEntityBase> writServices)
        {
            if (writServices == null) throw new ArgumentNullException(nameof(writServices));
            writServices.ConfigureServices(services);
        }

        public static void AddHandler<TKey, TMessageValue, TMessageHandler>(this IServiceCollection services)
            where TMessageValue : class
            where TMessageHandler : class, IObjectMessageHandler<TKey, TMessageValue>
        {
            services.AddTransient<IObjectMessageHandler<TKey, TMessageValue>, TMessageHandler>();

            var envelopeType = typeof(MessageEnvelope<>).MakeGenericType(typeof(TMessageValue));
            var envelopedHandlerType = typeof(IMessageHandler<,>).MakeGenericType(typeof(TKey), envelopeType);
            services.AddTransient(envelopedHandlerType, typeof(EnvelopedObjectMessageHandler<TKey, TMessageValue>));
        }

        public static WritKafkaServices<TKey, TEntityBase> UseCorrelationProvider<TKey, TEntityBase>(this WritKafkaServices<TKey, TEntityBase> writServices, CorrelationProvider correlationProvider)
        {
            writServices.CorrelationProvider = correlationProvider ?? throw new ArgumentNullException(nameof(correlationProvider));
            return writServices;
        }

        public static WritKafkaServices<TKey, TEntityBase> UseKeySerializers<TKey, TEntityBase>(this WritKafkaServices<TKey, TEntityBase> writServices,
            Type serializerType, Type deserializerType)
        {
            writServices.KeySerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
            writServices.KeyDeserializerType = deserializerType ?? throw new ArgumentNullException(nameof(deserializerType));
            return writServices;
        }

        public static WritKafkaServices<TKey, TEntityBase> UseKeySerialization<TKey, TEntityBase, TSerializer>(this WritKafkaServices<TKey, TEntityBase> writServices)
            where TSerializer : ISerializer<TKey>, IDeserializer<TKey>
        {
            return writServices.UseKeySerializers(typeof(TSerializer), typeof(TSerializer));
        }

        public static WritKafkaServices<TKey, TEntityBase> UseObjectSerializers<TKey, TEntityBase>(this WritKafkaServices<TKey, TEntityBase> writServices,
            Type serializerType, Type deserializerType)
        {
            writServices.ObjectSerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
            writServices.ObjectDeserializerType = deserializerType ?? throw new ArgumentNullException(nameof(deserializerType));
            return writServices;
        }

        public static WritKafkaServices<TKey, TEntityBase> UseObjectSerialization<TKey, TEntityBase, TSerializer>(this WritKafkaServices<TKey, TEntityBase> writServices)
            where TSerializer: ISerializer<TEntityBase>, IDeserializer<TEntityBase>
        {
            return writServices.UseObjectSerializers(typeof(TSerializer), typeof(TSerializer));
        }

        public static WritKafkaServices<TKey, TEntityBase> UseTopicConvention<TKey, TEntityBase>(this WritKafkaServices<TKey, TEntityBase> writServices,
            EntityTopicConvention topicConvention)
        {
            writServices.TopicConvention = topicConvention;
            return writServices;
        }

        public static WritKafkaServices<TKey, TEntityBase> UseKeyConvention<TKey, TEntityBase>(this WritKafkaServices<TKey, TEntityBase> writServices,
            EntityKeyConvention<TEntityBase, TKey> keyConvention)
        {
            writServices.KeyConvention = keyConvention;
            return writServices;
        }
    }

    public class WritKafkaServices<TKey, TEntityBase> : IApplicationNameResolver
    {
        public string ApplicationName { get; }
        public CorrelationProvider CorrelationProvider { get; set; }
        public bool UseEnvelopes { get; set; }
        public KafkaConfig KafkaConfig { get; }
        public Type KeySerializerType { get; set; }
        public Type KeyDeserializerType { get; set; }
        public Type ObjectSerializerType { get; set; }
        public Type ObjectDeserializerType { get; set; }
        public EntityTopicConvention TopicConvention { get; set; }
        public EntityKeyConvention<TEntityBase, TKey> KeyConvention { get; set; }

        public WritKafkaServices(string applicationName, KafkaConfig kafkaConfig)
        {
            ApplicationName = applicationName;
            KafkaConfig = kafkaConfig ?? throw new ArgumentNullException(nameof(kafkaConfig));

            // Set defaults
            CorrelationProvider = () => Guid.NewGuid().ToString(); 
            UseEnvelopes = true;
            KeyConvention = MessageConventions.DefaultKeyConvention<TEntityBase, TKey>();
            TopicConvention = MessageConventions.DefaultTopic;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this);
            services.AddSingleton<IApplicationNameResolver>(this);
            services.AddSingleton(p => KafkaConfig);
            services.AddScoped(p => CorrelationProvider);
            services.AddTransient<IEnvelopeHandler, EnvelopeHandler>();

            if (KeySerializerType != null)
                services.AddSingleton(typeof(ISerializer<TKey>), KeySerializerType);
            if (KeyDeserializerType != null)
                services.AddSingleton(typeof(IDeserializer<TKey>), KeyDeserializerType);

            if (ObjectSerializerType != null)
                services.AddSingleton(typeof(ISerializer<TEntityBase>), ObjectSerializerType);
            if (ObjectDeserializerType != null)
                services.AddSingleton(typeof(IDeserializer<TEntityBase>), ObjectDeserializerType);

            if (TopicConvention != null)
                services.AddSingleton(TopicConvention);
            if (KeyConvention != null)
                services.AddSingleton(KeyConvention);

            services.AddTransient<ISerializingProducer<TKey, TEntityBase>>(
                p => new Producer<TKey, TEntityBase>(
                    p.GetRequiredService<KafkaConfig>(),
                    p.GetRequiredService<ISerializer<TKey>>(),
                    p.GetRequiredService<ISerializer<TEntityBase>>()
                ));

            // Register decorator chain ConventionalObjectMessageProducer → EnvelopedObjectMessageProducer → ObjectMessageProducer
            services.AddTransient<ObjectMessageProducer<TKey>>();
            services.AddTransient(p => new EnvelopedObjectMessageProducer<TKey>(
                p.GetRequiredService<ObjectMessageProducer<TKey>>(), 
                p.GetRequiredService<IEnvelopeHandler>())
                );
            services.AddTransient(p => new ConventionalObjectMessageProducer<TKey, TEntityBase>(
                p.GetService<EnvelopedObjectMessageProducer<TKey>>(), 
                p.GetRequiredService<EntityTopicConvention>(), 
                p.GetRequiredService<EntityKeyConvention<TEntityBase, TKey>>(), 
                p.GetService<ILogger<ConventionalObjectMessageProducer<TKey, TEntityBase>>>()
                ));

            services.AddTransient<DispatchingConsumer<TKey, TEntityBase>>();
            services.AddTransient<Consumer<TKey, TEntityBase>, DispatchingConsumer<TKey, TEntityBase>>();                       
        }
    }
}
