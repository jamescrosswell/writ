using System;
using Writ.Messaging.Kafka;

namespace OptimisticKafka
{
    public static class MessageConventions
    {
        /// <summary>
        /// Determines an appropriate message key when writing an entity to Kafka
        /// </summary>
        public static EntityKeyConvention<string, object> Key = (object value) =>
        {
            var entity = value as IEntity;
                return (entity?.Id ?? Guid.Empty).ToString();
        };

    /// <summary>
        /// Determines an appropriate topic for writing an entity to Kafka
        /// </summary>
        public static EntityTopicConvention Topic = (Type type) => type.Name;
    }
}
