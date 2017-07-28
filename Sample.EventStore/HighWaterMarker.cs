using System;
using Confluent.Kafka;
using LiteDB;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore
{
    /// <summary>
    /// Helper class containing static methods that can be used to assist determining whether commands
    /// or events have already been handled (based on the TopicPartitionOffset).
    /// </summary>
    /// <remarks>
    /// You could also use Kafka consumer offsets to do this, however our sample application stores 
    /// application state in memory so it wants to be able to reprocess events every time it loads. At
    /// the same time it doesn't want to process commands more than once... so it needs to know when it
    /// has already processed an event. 
    /// </remarks>
    public static class HighWaterMarker
    {
        /// <summary>
        /// Make sure we don't process commands twice. This might happen after restarting the command processor
        /// if we're using an in memory application state.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool MessageAlreadyHandled<TKey, TValue>(this ApplicationState state, Message<TKey, TValue> message)
        {
            var highWaterMark = state.HighWaterMarks.FindOne(x => x.TopicPartition == message.TopicPartition);
            return (highWaterMark?.Offset >= message.Offset);
        }

        public static void RecordHighWaterMarks<TAggregateRoot, TKey, TValue>(
            this ApplicationState state, 
            Message<TKey, TValue> message, 
            IEvent<TAggregateRoot, TKey> fact
            )
            where TAggregateRoot : IAggregateRoot<TKey>
        {
            state.RecordHighWaterMark(message.TopicPartition, message.Offset);
            state.RecordHighWaterMark(fact.CommandOffset.TopicPartition, fact.CommandOffset.Offset);
        }

        public static void RecordHighWaterMark<TAggregateRoot, TKey>(this ApplicationState state, IEvent<TAggregateRoot, TKey> fact) 
            where TAggregateRoot : IAggregateRoot<TKey>
        {
            state.RecordHighWaterMark(fact.CommandOffset.TopicPartition, fact.CommandOffset.Offset);
        }

        public static void RecordHighWaterMark<TKey, TValue>(this ApplicationState state, Message<TKey, TValue> message)
        {
            state.RecordHighWaterMark(message.TopicPartition, message.Offset);
        }

        private static void RecordHighWaterMark(this ApplicationState state, TopicPartition topicPartition, Offset offset)
        {
            var currentMark = state.HighWaterMarks.FindOne(x => x.TopicPartition == topicPartition)
                            ?? new HighWaterMark { TopicPartition = topicPartition, Offset = -1 };
            if (offset > currentMark.Offset)
            {
                currentMark.Offset = offset;
                state.HighWaterMarks.Upsert(currentMark);
            }
        }
    }

    /// <summary>
    /// Stores the index of the last processed command for a particular topic. This 
    /// is useful to ensure commands don't get processed twice.
    /// </summary>
    public class HighWaterMark
    {
        //private Guid Id { get; set; }
        [BsonId]
        public TopicPartition TopicPartition { get; set; }
        public long Offset { get; set; }
    }
}