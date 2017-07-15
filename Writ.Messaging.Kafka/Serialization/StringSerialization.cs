using System.Text;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;

namespace Writ.Messaging.Kafka.Serialization
{
    /// <summary>
    /// A String implementation of the <see cref="ISerializer{T}"></see> interface /> and
    /// <see cref="IDeserializer{T}"/> interfaces for Kafka.
    /// </summary>
    public class StringSerialization : ISerializer<string>, IDeserializer<string>
    {
        private readonly Encoding _encoding;
        private readonly ISerializer<string> _stringSerializer = new StringSerializer(Encoding.UTF8);
        private readonly IDeserializer<string> _stringDeserializer = new StringDeserializer(Encoding.UTF8);

        static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        static StringSerialization()
        {
            SerializerSettings.TypeNameHandling = TypeNameHandling.All;
        }

        public StringSerialization(Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.UTF8;
        }

        public byte[] Serialize(string data)
        {
            return data == null ? null : _encoding.GetBytes(data);
        }

        public string Deserialize(byte[] data)
        {
            return data == null ? null : _encoding.GetString(data);
        }
    }
}
