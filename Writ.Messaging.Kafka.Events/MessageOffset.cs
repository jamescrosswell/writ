using Confluent.Kafka;

namespace Writ.Messaging.Kafka.Events
{
    /// <summary>
    /// A minimal serializable subset of the properties we need from TopicPartitionOffset
    /// </summary>
    public class MessageOffset
    {
        public string Topic { get; set; }
        public int Partition { get; set; }
        public long Offset { get; set; }

        public TopicPartition TopicPartition => new TopicPartition(Topic, Partition);

        public MessageOffset()
        {
            
        }

        public MessageOffset(TopicPartitionOffset topicPartitionOffset)
        {
            Topic = topicPartitionOffset.Topic;
            Partition = topicPartitionOffset.Partition;
            Offset = topicPartitionOffset.Offset;
        }

        public static implicit operator MessageOffset(TopicPartitionOffset topicPartitionOffset)
        {
            return new MessageOffset(topicPartitionOffset);
        }

        public static implicit operator TopicPartitionOffset(MessageOffset messageOffset)
        {
            return new TopicPartitionOffset(messageOffset.Topic, messageOffset.Partition, messageOffset.Offset);
        }
    }
}