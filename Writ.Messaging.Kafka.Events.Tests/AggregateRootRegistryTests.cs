using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Writ.Messaging.Kafka.Events.Tests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public class AggregateRootRegistryTests
    {
        private sealed class Fixture
        {
            public AggregateRootRegistry GetSut()
            {
                var aggregates = new AggregateRootRegistry();
                aggregates.RegisterAggregate<Test>();
                return aggregates;
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void TopicConvention_ReturnsAggregateCommandTopic()
        {
            // Given an aggregate root
            var sut = _fixture.GetSut();

            // When I get the topic for a command
            var result = sut.TopicConvention(typeof(CreateTest));

            // The result is the aggregate command topic
            var aggregateInfo = sut.GetAggregateRootInfo<Test>();
            Assert.Equal(aggregateInfo.CommandTopic, result);
        }

        [Fact]
        public void TopicConvention_ReturnsAggregateCommandTopicForInterface()
        {
            // Given an aggregate root
            var sut = _fixture.GetSut();

            // When I get the topic for a command interface
            var result = sut.TopicConvention(typeof(ICommand<Test, int>));

            // The result is the aggregate command topic
            var aggregateInfo = sut.GetAggregateRootInfo<Test>();
            Assert.Equal(aggregateInfo.CommandTopic, result);
        }

        [Fact]
        public void TopicConvention_ReturnsAggregateEventTopic()
        {
            // Given an aggregate root
            var sut = _fixture.GetSut();

            // When I get the topic for an event
            var result = sut.TopicConvention(typeof(TestCreated));

            // The result is the aggregate event topic
            var aggregateInfo = sut.GetAggregateRootInfo<Test>();
            Assert.Equal(aggregateInfo.EventTopic, result);
        }

        [Fact]
        public void TopicConvention_ReturnsAggregateEventTopicForInterface()
        {
            // Given an aggregate root
            var sut = _fixture.GetSut();

            // When I get the topic for an event interface
            var result = sut.TopicConvention(typeof(IEvent<Test, int>));

            // The result is the aggregate event topic
            var aggregateInfo = sut.GetAggregateRootInfo<Test>();
            Assert.Equal(aggregateInfo.EventTopic, result);
        }

        [Fact]
        public void TopicConvention_ReturnsCommandFailureTopic()
        {
            // Given an aggregate root
            var sut = _fixture.GetSut();

            // When I get the topic for a command failure
            var result = sut.TopicConvention(typeof(CommandFailure<Test, int>));

            // The result is the command failure back channel
            Assert.Equal(sut.CommandFailureTopic, result);
        }

        private class Test : IAggregateRoot<int>
        {
            public int Id { get; }
        }

        private class CreateTest : ICommand<Test, int>
        {
            public int Id { get; }
        }

        private class TestCreated : IEvent<Test, int>
        {
            public int Id { get; }
        }
    }
}
