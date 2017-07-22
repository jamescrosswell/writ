using System;
using System.Collections.Generic;

namespace Writ.Messaging
{
    /// <inheritdoc cref="ISchemaTypeMap"/>
    public class SchemaTypeMap : ISchemaTypeMap
    {
        private readonly Dictionary<Type, SpecificSchema> _typeSchemas = new Dictionary<Type, SpecificSchema>();
        private readonly Dictionary<SpecificSchema, Type> _schemaTypes = new Dictionary<SpecificSchema, Type>();


        public void RegisterTypeSchema<T>(string subject)
        {
            RegisterTypeSchema<T>(subject, 1);
        }

        public void RegisterTypeSchema<T>(string subject, int version)
        {
            RegisterTypeSchema<T>(new SpecificSchema(subject, version));
        }

        public void RegisterTypeSchema<T>(SpecificSchema schema)
        {
            if (_schemaTypes.ContainsKey(schema))
                throw new ArgumentException("Schema already registered", nameof(schema));
            var type = typeof(T);
            RegisterTypeSchema(type, schema);
        }

        public void RegisterTypeSchema(Type type, string subject, int version)
        {
            RegisterTypeSchema(type, new SpecificSchema(subject, version));
        }

        public void RegisterTypeSchema(Type type, SpecificSchema schema)
        {
            // Specific Schemas should only be registered once
            if (_schemaTypes.ContainsKey(schema))
                throw new ArgumentException("Schema already registered", nameof(schema));

            // Specific Schemas are always associated with a type (e.g. when deserializing)
            _schemaTypes[schema] = type;

            // Types can be associated with multiple schemas, so we use the latest schema as the default (e.g. when serializing)
            if (!_typeSchemas.TryGetValue(type, out var currentDefault) || currentDefault.Version < schema.Version)
                _typeSchemas[type] = schema;

        }

        public SpecificSchema GetSchema(Type type) => _typeSchemas[type];
        public Type GetType(SpecificSchema schema) => _schemaTypes[schema];
    }
}
