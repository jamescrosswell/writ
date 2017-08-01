using Confluent.Kafka;
using System;

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

        public MessageOffset(string topic, int partition, long offset)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Partition = partition;
            Offset = offset;
        }

        public MessageOffset(TopicPartitionOffset topicPartitionOffset)
            : this(topicPartitionOffset.Topic, topicPartitionOffset.Partition, topicPartitionOffset.Offset)
        {
        }

        public static implicit operator MessageOffset(TopicPartitionOffset topicPartitionOffset)
        {
            return new MessageOffset(topicPartitionOffset);
        }

        public static implicit operator TopicPartitionOffset(MessageOffset messageOffset)
        {
            return new TopicPartitionOffset(messageOffset.Topic, messageOffset.Partition, messageOffset.Offset);
        }


        protected bool Equals(MessageOffset other)
        {
            return 
                Topic.Equals(other.Topic) 
                && Equals(Partition, other.Partition)
                && Equals(Offset, other.Offset);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageOffset)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Topic.GetHashCode() * 397) ^ Partition.GetHashCode() ^ Offset.GetHashCode();
            }
        }
    }
}