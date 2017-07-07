namespace OptimisticKafka
{
    public static class MessageConventions
    {
        /// <summary>
        /// Determines an appropriate message key when writing an entity to Kafka
        /// </summary>
        /// <param name="value">The entity being written</param>
        /// <returns>A key name</returns>
        public static string Key(IEntity value) => value.Id.ToString();

        /// <summary>
        /// Determines an appropriate topic when writing an entity to Kafka
        /// </summary>
        /// <param name="value">The entity being written</param>
        /// <returns>A topic name</returns>
        public static string Topic(IEntity value) => value.GetType().Name;
    }
}
