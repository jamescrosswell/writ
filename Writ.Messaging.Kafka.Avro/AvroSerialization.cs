using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Confluent.Kafka.Serialization;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;

namespace Writ.Messaging.Kafka.Avro
{
    /// <summary>
    /// An implementation of the <see cref="ISerializer{T}"></see> and 
    /// <see cref="IDeserializer{T}"/> intefaces for Kafka using Avro.
    /// </summary>
    public class AvroSerialization : ISerializer<object>, IDeserializer<object>
    {
        private const char SchemaIdDelimiter = '\r';
        private readonly ISchemaTypeMap _schemaTypeMap;

        public AvroSerialization(ISchemaTypeMap schemaTypeMap)
        {
            _schemaTypeMap = schemaTypeMap;
        }

        private static dynamic GetTypeSerializer(Type type)
        {
            var serializerFactoryMethod = typeof(AvroSerializer)
                .GetTypeInfo()
                .GetMethod("Create", new Type[] { })
                .MakeGenericMethod(type);
            return serializerFactoryMethod.Invoke(null, null);
        }

        public byte[] Serialize(object data)
        {
            using (var stream = new MemoryStream())
            {
                // Write the schema at the start of the message
                var schema = _schemaTypeMap.GetSchema(data.GetType());
                var schemaId = Encoding.ASCII.GetBytes(schema.ToString() + SchemaIdDelimiter);
                stream.Write(schemaId, 0, schemaId.Length);

                // Now write out the payload
                var typeSerializer = GetTypeSerializer(data.GetType());
                var serializeMethod = typeof(IAvroSerializer<>)
                    .MakeGenericType(data.GetType())
                    .GetMethodByName("Serialize", typeof(Stream), data.GetType());
                serializeMethod.Invoke(typeSerializer, new[] { stream, data });
                
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                // Read the schema from the start of the message
                var sb = new StringBuilder();
                while (true)
                {
                    var currentChar = (char)stream.ReadByte();
                    if (currentChar == SchemaIdDelimiter)
                        break;
                    sb.Append(currentChar);
                }
                var schema = SpecificSchema.Parse(sb.ToString());
                var payloadType = _schemaTypeMap.GetType(schema);

                // Now read the payload
                var typeSerializer = GetTypeSerializer(payloadType);
                var deserializeMethod = typeof(IAvroSerializer<>)
                    .MakeGenericType(payloadType)
                    .GetMethodByName("Deserialize", typeof(Stream));
                object[] parameters = { stream };
                return deserializeMethod.Invoke(typeSerializer, parameters);
            }
        }
    }
}
