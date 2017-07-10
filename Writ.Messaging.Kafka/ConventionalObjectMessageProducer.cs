using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Writ.Messaging.Kafka
{
    public interface IObjectMessageProducer<in TEntity> : IObjectMessageProducer
    {
        Task<Message<string, object>> ProduceAsync(TEntity value);
    }

    public delegate string EntityTopicConvention(Type entityType);

    /// <summary>
    /// This decorator makes it easy to implement conventions regarding message keys and topic names 
    /// when writing objects to kafka.
    /// </summary>
    public class ConventionalObjectMessageProducer<TEntity> : IObjectMessageProducer<TEntity>
    {
        public delegate string EntityKeyConvention(TEntity entity);

        private readonly IObjectMessageProducer _wrappedProducer;
        private readonly EntityKeyConvention _keyConvention;
        private readonly EntityTopicConvention _topicConvention;
        private readonly ILogger<ConventionalObjectMessageProducer<TEntity>> _logger;
        public string Name => _wrappedProducer.Name;

        /// <summary>
        /// Creates a ConventionalObjectMessageProducer
        /// </summary>
        /// <param name="producer">An object message producer to wrap</param>
        /// <param name="keyConvention">Func that determines an appropriate message key</param>
        /// <param name="topicConvention">Func that determines an appropriate topic</param>
        /// <param name="logger">Logger factory</param>
        public ConventionalObjectMessageProducer(
            IObjectMessageProducer producer,
            EntityKeyConvention keyConvention,
            EntityTopicConvention topicConvention,
            ILogger<ConventionalObjectMessageProducer<TEntity>> logger)
        {
            _wrappedProducer = producer ?? throw new ArgumentNullException(nameof(producer));
            _keyConvention = keyConvention ?? throw new ArgumentNullException(nameof(keyConvention));
            _topicConvention = topicConvention ?? throw new ArgumentNullException(nameof(topicConvention));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Message<string, object>> ProduceAsync(TEntity value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var key = _keyConvention(value);
            var topic = _topicConvention(typeof(TEntity));

            _logger.LogDebug($"{Name} resolved topic {topic} for messsage key {key}.");
            return ProduceAsync(topic, key, value);
        }

        public Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value)
        {
            return _wrappedProducer.ProduceAsync(topic, key, value);
        }

        public void Dispose()
        {
            _wrappedProducer?.Dispose();
        }

    }
}