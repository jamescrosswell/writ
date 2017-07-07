using System;

namespace OptimisticKafka
{
    /// <summary>
    /// An Entity
    /// </summary>
    public interface IEntity
    {
        Guid Id { get; }
    }
}
