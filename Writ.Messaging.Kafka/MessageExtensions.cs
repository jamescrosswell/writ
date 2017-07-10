using Confluent.Kafka;

namespace Writ.Messaging.Kafka
{
    public static class MessageExtensions
    {
        public static Message<TKey, TDestValue> Repackage<TKey, TSourceValue, TDestValue>(
            this Message<TKey, TSourceValue> source, TDestValue value)
        { 
            return new Message<TKey, TDestValue>(
                source.Topic,
                source.Partition,
                source.Offset,
                source.Key,
                value,
                source.Timestamp,
                source.Error
            );
        }
    }
}