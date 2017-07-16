using System;
using System.Linq;
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
        /// <typeparam name="TKey">They type of the key used for the key convention</typeparam>
        /// <returns>A default key convention for <typeparamref name="TEntity"/></returns>
        public static EntityKeyConvention<TEntity, TKey> DefaultKeyConvention<TEntity, TKey>()
        {
            return entity =>
            {
                var idProperty = entity.GetType().GetProperty("Id");
                var value = idProperty.GetValue(entity);
                return (TKey) Convert.ChangeType(value, typeof(TKey));
            };
        }

        /// <summary>
        /// Creates a key convention assuming an id property of "Id" and type "Guid"
        /// </summary>
        /// <typeparam name="TEntity">The type of entity that you want a key convention for</typeparam>
        /// <returns>A Guid key convention for <typeparamref name="TEntity"/></returns>
        public static EntityKeyConvention<TEntity, Guid> GuidKeyConvention<TEntity>()
        {
            return entity =>
            {
                var idProperty = entity.GetType().GetProperty("Id");
                return (Guid)idProperty.GetValue(entity);
            };
        }

        /// <summary>
        /// Creates a key convention assuming an id property that can be converted to and parsed from 
        /// a string
        /// </summary>
        /// <typeparam name="TEntity">The type of entity that you want a key convention for</typeparam>
        /// <returns>A String key convention for <typeparamref name="TEntity"/></returns>
        public static EntityKeyConvention<TEntity, string> StringKeyConvention<TEntity>()
        {
            return entity =>
            {
                var idProperty = entity.GetType().GetProperty("Id");
                return idProperty.GetValue(entity).ToString();
            };
        }

        /// <summary>
        /// Determines an appropriate topic when writing an entity to Kafka
        /// </summary>
        public static EntityTopicConvention DefaultTopic = (Type type) => type.Name;
    }
}
