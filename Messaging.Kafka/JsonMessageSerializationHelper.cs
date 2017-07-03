using System;
using System.Dynamic;
using System.Text;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messaging.Kafka
{
    /// <summary>
    /// A Json implementation of the <see cref="ISerializer{T}"></see> interface /> and
    /// <see cref="IDeserializer{T}"/> intefaces for Kafka.
    /// </summary>
    public class JsonMessageSerializationHelper : ISerializer<object>, IDeserializer<object>
    {
        private readonly ISerializer<string> _stringSerializer = new StringSerializer(Encoding.UTF8);
        private readonly IDeserializer<string> _stringDeserializer = new StringDeserializer(Encoding.UTF8);

        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        static JsonMessageSerializationHelper()
        {
            SerializerSettings.TypeNameHandling = TypeNameHandling.All;
        }

        public byte[] Serialize(object data)
        {
            var json = JsonConvert.SerializeObject(data, SerializerSettings);
            return _stringSerializer.Serialize(json);
        }

        public object Deserialize(byte[] data)
        {
            var json = _stringDeserializer.Deserialize(data);
            return JsonConvert.DeserializeObject(json, SerializerSettings);
        }
    }    
}
