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

        /// <summary>
        /// Creates a ConventionalObjectMessageProducer
        /// </summary>
        /// <param name="producer">An object message producer to wrap</param>
        /// <param name="keyResolver">Func that determines an appropriate message key</param>
        /// <param name="topicResolver">Func that determines an appropriate topic</param>
        /// <param name="loggerFactory">Logger factory</param>
        public ConventionalObjectMessageProducer(IObjectMessageProducer producer, Func<TEntity, string> keyResolver, Func<TEntity, string> topicResolver, ILoggerFactory loggerFactory)
        {
            _wrappedProducer = producer ?? throw new ArgumentNullException(nameof(producer));
            _keyResolver = keyResolver;
            _topicResolver = topicResolver;
            _logger = loggerFactory?.CreateLogger<ConventionalObjectMessageProducer<TEntity>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Task<Message<string, object>> ProduceAsync(TEntity value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (_topicResolver == null)
                throw new InvalidOperationException("Unable to determine an appropriate topic for the message. No topic resolver was supplied.");

            var topic = _topicResolver(value);
            return ProduceAsync(topic, value);
        }

        public Task<Message<string, object>> ProduceAsync(string topic, TEntity value)
        {
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (_keyResolver == null)
                throw new InvalidOperationException("Unable to determine an appropriate key for the message. No key resolver was supplied.");

            var key = _keyResolver(value);
            return ProduceAsync(topic, key, value);
        }

        public Task<Message<string, object>> ProduceAsync<TMessage>(string topic, string key, TMessage value)
        {
            _logger.LogDebug($"{Internal.Name} producing on {topic}.");
            return _wrappedProducer.ProduceAsync(topic, key, value);
        }

        public void Dispose()
        {
            _wrappedProducer?.Dispose();
        }

        public ProducerProxy Internal => _wrappedProducer.Internal;
    }
}