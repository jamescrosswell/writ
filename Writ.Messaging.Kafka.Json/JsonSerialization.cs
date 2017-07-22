using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace Writ.Messaging.Kafka.Serialization
{
    /// <summary>
    /// A Json implementation of the <see cref="ISerializer{T}"></see> interface /> and
    /// <see cref="IDeserializer{T}"/> intefaces for Kafka.
    /// </summary>
    public class JsonSerialization : ISerializer<object>, IDeserializer<object>
    {
        private readonly ISchemaTypeMap _schemaTypeMap;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        static JsonSerialization()
        {
            SerializerSettings.TypeNameHandling = TypeNameHandling.None;
        }

        public JsonSerialization(ISchemaTypeMap schemaTypeMap)
        {
            _schemaTypeMap = schemaTypeMap ?? throw new ArgumentNullException(nameof(schemaTypeMap));
        }

        public byte[] Serialize(object data)
        {
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    // Write the schema at the start of the message
                    var schema = _schemaTypeMap.GetSchema(data.GetType());
                    streamWriter.WriteLine(schema.ToString());

                    // Now write out the payload
                    var json = JsonConvert.SerializeObject(data, SerializerSettings);
                    streamWriter.Write(json);
                }

                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    // Read the schema from the start of the message
                    var schemaId = streamReader.ReadLine();
                    var schema = SpecificSchema.Parse(schemaId);
                    var payloadType = _schemaTypeMap.GetType(schema);

                    // Now read out the payload
                    var json = streamReader.ReadToEnd();
                    var deserializeMethod = typeof(JsonConvert)
                        .GetMethods()
                        .Single(
                            m => m.Name == "DeserializeObject"
                                 && m.IsGenericMethod
                                 && m.GetParameters().Length == 2
                                 && m.GetParameters()[0].ParameterType == typeof(string)
                                 && m.GetParameters()[1].ParameterType == typeof(JsonSerializerSettings)
                        )
                        .MakeGenericMethod(payloadType);
                    object[] parameters = { json, SerializerSettings };
                    return deserializeMethod.Invoke(null, parameters);
                }
            }

        }
    }
}
