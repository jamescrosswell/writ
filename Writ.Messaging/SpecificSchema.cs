using System;

namespace Writ.Messaging
{
    /// <summary>
    /// Identifies the specific version of a message schema that a message contains, by combining 
    /// a namespaced schema name and a schema version number. Typically the specific schema would 
    /// be written out at the beginning/header of a message during serialization so that the 
    /// corresponding deserilizer can infer message type using a <see cref="ISchemaTypeMap"/>.
    /// </summary>
    public struct SpecificSchema
    {
        public SpecificSchema(string name, int version)
        {
            Name = name;
            Version = version;
        }
        public string Name { get; set; }
        public int Version { get; set; }

        public override string ToString() => $"{Name},{Version}";

        public static SpecificSchema Parse(string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[1], out int version))
                throw new ArgumentException(nameof(text));

            var name = parts[0];
            return new SpecificSchema(name, version);
        }
    }
}