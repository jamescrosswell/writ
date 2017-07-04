using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Messaging.Kafka
{
    public interface IObjectMessageProducer<in TEntity> : IObjectMessageProducer
    {
        Task<Message<string, object>> ProduceAsync(TEntity value);
    }

    /// <summary>
    /// This decorator makes it easy to implement conventions regarding message keys and topic names 
    /// when writing objects to kafka.
    /// </summary>
    public class ConventionalObjectMessageProducer<TEntity> : IObjectMessageProducer<TEntity>
    {
        private readonly IObjectMessageProducer _wrappedProducer;
        private readonly Func<TEntity, string> _keyResolver;
        private readonly Func<TEntity, string> _topicResolver;
        private readonly ILogger<ConventionalObjectMessageProducer<TEntity>> _logger;
        public string Name => _wrappedProducer.Name;

        /// <summary>
        /// Creates a ConventionalObjectMessageProducer
        /// </summary>
        /// <param name="producer">An object message producer to wrap</param>
        /// <param name="keyResolver">Func that determines an appropriate message key</param>
        /// <param name="topicResolver">Func that determines an appropriate topic</param>
        /// <param name="logger">Logger factory</param>
        public ConventionalObjectMessageProducer(
            IObjectMessageProducer producer, 
            Func<TEntity, string> keyResolver, 
            Func<TEntity, string> topicResolver,
            ILogger<ConventionalObjectMessageProducer<TEntity>> logger)
        {
            _wrappedProducer = producer ?? throw new ArgumentNullException(nameof(producer));
            _keyResolver = keyResolver ?? throw new ArgumentNullException(nameof(keyResolver));
            _topicResolver = topicResolver ?? throw new ArgumentNullException(nameof(topicResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Message<string, object>> ProduceAsync(TEntity value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var key = _keyResolver(value);
            var topic = _topicResolver(value);

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