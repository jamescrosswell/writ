using System.Collections.Generic;

namespace Writ.Messaging.Kafka
{
    public class KafkaConfig : Dictionary<string, object>
    {
        public string AutoOffset
        {
            get => (string)this["auto.offset.reset"];
            set => this["auto.offset.reset"] = value;
        }

        public string BrokerList
        {
            get => (string)this["bootstrap.servers"];
            set => this["bootstrap.servers"] = value;
        }

        public string GroupId
        {
            get => (string)this["group.id"];
            set => this["group.id"] = value;
        }
    }
}
