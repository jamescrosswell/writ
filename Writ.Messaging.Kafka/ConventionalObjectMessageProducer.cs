using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Writ.Messaging.Kafka
{
    public interface IEntityMessageProducer<TKey, in TEntity> : IObjectMessageProducer<TKey>
    {
        Task<Message<TKey, object>> ProduceAsync<TMessage>(TMessage value) where TMessage : TEntity;
    }

    public delegate string EntityTopicConvention(Type entityType);
    public delegate TKey EntityKeyConvention<out TKey, in TEntity>(TEntity entity);

    /// <summary>
    /// This decorator makes it easy to implement conventions regarding message keys and topic names 
    /// when writing objects to kafka.
    /// </summary>
    public class ConventionalObjectMessageProducer<TKey, TEntity> : IEntityMessageProducer<TKey, TEntity>
    {
        //public delegate TKey EntityKeyConvention(TEntity entity);

        private readonly IObjectMessageProducer<TKey> _wrappedProducer;
        private readonly EntityKeyConvention<TKey, TEntity> _keyConvention;
        private readonly EntityTopicConvention _topicConvention;
        private readonly ILogger<ConventionalObjectMessageProducer<TKey, TEntity>> _logger;
        public string Name => _wrappedProducer.Name;

        /// <summary>
        /// Creates a ConventionalObjectMessageProducer
        /// </summary>
        /// <param name="producer">An object message producer to wrap</param>
        /// <param name="topicConvention">Func that determines an appropriate topic</param>
        /// <param name="keyConvention">Func that determines an appropriate message key</param>
        /// <param name="logger">Logger factory</param>
        public ConventionalObjectMessageProducer(
            IObjectMessageProducer<TKey> producer, 
            EntityTopicConvention topicConvention, 
            EntityKeyConvention<TKey, TEntity> keyConvention, 
            ILogger<ConventionalObjectMessageProducer<TKey, TEntity>> logger
            )
        {
            _wrappedProducer = producer ?? throw new ArgumentNullException(nameof(producer));
            _keyConvention = keyConvention ?? throw new ArgumentNullException(nameof(keyConvention));
            _topicConvention = topicConvention ?? throw new ArgumentNullException(nameof(topicConvention));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Message<TKey, object>> ProduceAsync<TMessage>(TMessage value)
            where TMessage: TEntity
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var key = _keyConvention(value);
            var topic = _topicConvention(typeof(TMessage));

            _logger.LogDebug($"{Name} resolved topic {topic} for messsage key {key}.");

            return ProduceAsync(topic, key, value);
        }

        public Task<Message<TKey, object>> ProduceAsync<TMessage>(string topic, TKey key, TMessage value)
        {
            return _wrappedProducer.ProduceAsync(topic, key, value);
        }

        public void Dispose()
        {
            _wrappedProducer?.Dispose();
        }

    }
}