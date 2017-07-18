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
            var genericInterfaceTypeDefinition = GetGenericInterfaceTypeDefinition(type, typeof(ICommand<,>));
            var aggregateRootType = genericInterfaceTypeDefinition?.GenericTypeArguments.First();
            return GetAggregateRootInfo(aggregateRootType)?.CommandTopic;
        }

        private string GetCommandFailureTopic(Type type)
        {
            var failureType = GetGenericInterfaceTypeDefinition(type, typeof(ICommandFailure<,>));
            return (failureType != null)
                ? CommandFailureTopic
                : null;
        }

        private string GetEventTopic(Type type)
        {
            var genericInterfaceTypeDefinition = GetGenericInterfaceTypeDefinition(type, typeof(IEvent<,>));
            var aggregateRootType = genericInterfaceTypeDefinition?.GenericTypeArguments.First();
            return GetAggregateRootInfo(aggregateRootType)?.EventTopic;
        }

        /// <summary>
        /// This one needs some commentary. We want to inspect the definition of a generic 
        /// interface for a type... however we're not sure if the type IS an interface type
        /// or if it's a class that IMPLEMENTS an interface. The code is necessarily a bit
        /// messy then and we don't want it poluting all the other methods here
        /// </summary>
        /// <param name="type">The interface or class type to inspect</param>
        /// <param name="interfaceType">The type of the generic interface that we want the type definition for</param>
        private Type GetGenericInterfaceTypeDefinition(Type type, Type interfaceType)
        {
            // type can be an interface and an interface doesn't implement itself.
            // typeof(IEvent<int,int>).GetInterfaces() does not contain IEvent<int,int>!
            var typeInfo = type.GetTypeInfo();
            var isInterfaceType = typeInfo.IsInterface && typeInfo.IsGenericType &&
                                  typeInfo.GetGenericTypeDefinition() == interfaceType;
            return isInterfaceType
                 ? type
                 // Alternatively the type might merely implement the interface we're looking for
                 : type
                    .GetInterfaces()
                    .SingleOrDefault(i => i.GetGenericTypeDefinition() == interfaceType);
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