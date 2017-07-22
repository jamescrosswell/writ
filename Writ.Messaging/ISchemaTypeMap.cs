using System;

namespace Writ.Messaging
{
    /// <summary>
    /// We need a way to create concreate types of messages when deserializing 
    /// these. The SchemaTypeMap lets us do this by mapping between the
    /// <see cref="SpecificSchema"/> an a corresponding <see cref="Type"/>. 
    /// </summary>
    public interface ISchemaTypeMap
    {
        SpecificSchema GetSchema(Type type);
        Type GetType(SpecificSchema schema);
        void RegisterTypeSchema(Type type, SpecificSchema schema);
        void RegisterTypeSchema(Type type, string subject, int version);
        void RegisterTypeSchema<T>(SpecificSchema schema);
        void RegisterTypeSchema<T>(string subject, int version);
    }
}