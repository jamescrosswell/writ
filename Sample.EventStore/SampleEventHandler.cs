using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore
{
    public abstract class SampleEventHandler<TAggregateRoot, TKey, TEvent> : ObjectMessageHandler<string, TEvent>
        where TAggregateRoot : IAggregateRoot<TKey>        
        where TEvent : class, IEvent<TAggregateRoot, TKey>
    {
        protected ILogger<SampleEventHandler<TAggregateRoot, TKey, TEvent>> Logger { get; }
        protected ApplicationState State { get; }
        protected Message<string, object> Message { get; private set; }

        protected SampleEventHandler(ApplicationState applicationState, ILogger<SampleEventHandler<TAggregateRoot, TKey, TEvent>> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            State = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        public abstract void Handle(TEvent value);

        public override void Handle(Message<string, object> message, TEvent value)
        {
            Message = message;
            using (var trans = State.BeginTrans())
            {
                Handle(value);
                State.RecordHighWaterMark(value);
                trans.Commit();
            }
        }
    }
}