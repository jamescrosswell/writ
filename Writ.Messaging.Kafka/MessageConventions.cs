using System;
using System.Reflection;

namespace Writ.Messaging.Kafka
{
    public static class MessageConventions
    {
        /// <summary>
        /// Creates a default key convention for a particular base class... the default convention
        /// simply looks for a property called "Id" and converts this to <typeparamref name="TKey"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity that you want a key convention for</typeparam>
        /// <returns>A default key convention for <typeparamref name="TEntity"/></returns>
        public static EntityKeyConvention<TEntity, TKey> DefaultKeyConvention<TEntity, TKey>()
        {
            return entity =>
            {
                var idProperty = entity.GetType().GetProperty("Id");
                var id = idProperty.GetValue(entity);
                return (TKey)Convert.ChangeType(id, typeof(TKey));
            };
        }

        /// <summary>
        /// Determines an appropriate topic when writing an entity to Kafka
        /// </summary>
        public static EntityTopicConvention DefaultTopic = (Type type) => type.Name;
    }
}
