using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Writ.Messaging.Kafka.Events
{
    public class AggregateRootRegistry
    {        
        public readonly string CommandFailureTopic = "_CommandFailures"; // TODO: Make the CommandFailureTopic configurable
        private readonly Dictionary<Type, AggregateRootInfo> _aggregates = new Dictionary<Type, AggregateRootInfo>();

        public class AggregateRootInfo
        {
            public AggregateRootInfo(string commandTopic, string eventTopic)
            {
                CommandTopic = commandTopic ?? throw new ArgumentNullException(nameof(commandTopic));
                EventTopic = eventTopic ?? throw new ArgumentNullException(nameof(eventTopic));
            }

            public string CommandTopic { get; }
            public string EventTopic { get; }
        }

        public string TopicConvention(Type type)
        {
            return 
                GetCommandTopic(type)
                ?? GetCommandFailureTopic(type)
                ?? GetEventTopic(type)
                ?? throw new Exception($"No Aggregate root or back channel registered for {type.FullName}");
        }

        private string GetCommandTopic(Type type)
        {
            var aggregateRootType = type.GetInterfaces()
                .SingleOrDefault(i => i.GetGenericTypeDefinition() == typeof(ICommand<,>))
                ?.GenericTypeArguments.First();
            return GetAggregateRootInfo(aggregateRootType)?.CommandTopic;
        }

        private string GetCommandFailureTopic(Type type)
        {
            var failureType = type.GetInterfaces()
                .SingleOrDefault(i => i.GetGenericTypeDefinition() == typeof(ICommandFailure<,>));
            return (failureType != null)
                ? CommandFailureTopic
                : null;
        }

        private string GetEventTopic(Type type)
        {
            var aggregateRootType = type.GetInterfaces()
                .SingleOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEvent<,>))
                ?.GenericTypeArguments.First();
            return GetAggregateRootInfo(aggregateRootType)?.EventTopic;
        }

        public AggregateRootInfo GetAggregateRootInfo<TAggregateRoot>()
        {
            return GetAggregateRootInfo(typeof(TAggregateRoot));
        }

        public AggregateRootInfo GetAggregateRootInfo(Type type)
        {
            return (type != null)
                ? _aggregates[type]
                : null;
        }

        public void RegisterAggregate<TAggregate>()
        {
            var topicSuffix = typeof(TAggregate).Name;
            AggregateRootInfo info = new AggregateRootInfo(
                $"Commands.{topicSuffix}",
                $"Events.{topicSuffix}"
            );
            RegisterAggregate<TAggregate>(info);
        }

        public void RegisterAggregate<TAggregate>(AggregateRootInfo info)
        {

            _aggregates[typeof(TAggregate)] = info;
        }

        public IEnumerable<string> GetEventTopics(bool includeBackChannel)
        {
            if (includeBackChannel)
                yield return CommandFailureTopic;
            foreach (var aggregateInfo in _aggregates.Values)
                yield return aggregateInfo.EventTopic;
        }

        public IEnumerable<string> GetCommandTopics() => _aggregates.Values.Select(x => x.CommandTopic);
    }
}