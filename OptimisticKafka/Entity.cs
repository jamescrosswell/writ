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

    /// <summary>
    /// An Entity
    /// </summary>
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Gets the identifier of the Entity
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; }

        protected Entity(Guid id)
        {
            if (id.Equals(default(Guid))) throw new ArgumentException(nameof(id));
            Id = id;
        }

        protected Entity()
            : this(Guid.NewGuid())
        {
            
        }
    }
}
