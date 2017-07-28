using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Writ.Messaging.Kafka;
using Writ.Messaging.Kafka.Events;

namespace Sample.EventStore
{
    public abstract class SampleCommandHandler<TAggregateRoot, TKey, TCommand, TEvent> : ObjectMessageHandler<string, TCommand>
        where TAggregateRoot : IAggregateRoot<TKey>
        where TCommand : class, ICommand<TAggregateRoot, TKey>
        where TEvent: IEvent<TAggregateRoot, TKey>
    {
        protected readonly ILogger<SampleCommandHandler<TAggregateRoot, TKey, TCommand, TEvent>> Logger;
        protected readonly IObjectMessageHandler<string, TEvent> FactHandler;
        protected readonly ConventionalObjectMessageProducer<string, object> Producer;
        protected readonly ApplicationState State;

        protected SampleCommandHandler(
            ApplicationState applicationState,
            ConventionalObjectMessageProducer<string, object> producer,
            IObjectMessageHandler<string, TEvent> factHandler,
            ILogger<SampleCommandHandler<TAggregateRoot, TKey, TCommand, TEvent>> logger
        )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            FactHandler = factHandler ?? throw new ArgumentNullException(nameof(factHandler));
            Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            State = applicationState ?? throw new ArgumentNullException(nameof(applicationState));
        }

        protected abstract IEnumerable<CommandFailure<TCommand, TAggregateRoot, TKey>> ValidateCommand(Message<string, object> message, TCommand value);

        protected abstract TEvent ProcessCommand(Message<string, object> message, TCommand value);

        public sealed override void Handle(Message<string, object> message, TCommand value)
        {
            // Make sure we don't process commands twice. 
            if (State.MessageAlreadyHandled(message))
                return;

            // Validate the command can be run given the current application state. 
            // For example, we might check the command wouldn't result in duplicates values for 
            // a property where we have a constraint for uniqueness in our domain model.
            var failures = ValidateCommand(message, value).ToList();
            if (failures.Any())
            {
                failures.ForEach(failure => Producer.ProduceAsync(failure));
                return;
            }

            // If we get to here then the command *must* be processed... which means it becomes a 
            // fact published to the event stream
            var fact = ProcessCommand(message, value);
            Producer.ProduceAsync(fact);

            // Finally, we apply the fact to the command processor's state
            FactHandler.Handle(message, fact); 
        }
    }
}