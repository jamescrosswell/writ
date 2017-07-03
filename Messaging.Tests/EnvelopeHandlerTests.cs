using System;
using NSubstitute;
using Xunit;

namespace Messaging.Tests
{
    public class EnvelopeHandlerTests
    {
        private sealed class Fixture
        {
            public string ApplicationName { get; } = "Messaging.Tests";
            public ICorrelationIdProvider CorrelationIdProvider { get; } = Substitute.For<ICorrelationIdProvider>();

            public EnvelopeHandler GetSut()
            {
                return new EnvelopeHandler(ApplicationName, CorrelationIdProvider);
            }
        }    

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Stuff_ReturnsMessageEnvelope()
        {
            var message = new Object();
            var correlationId = Guid.NewGuid().ToString();
            _fixture.CorrelationIdProvider.GetCurrentCorrelationId().Returns(correlationId);
            var sut = _fixture.GetSut();

            var result = sut.Stuff(message);

            Assert.Equal(correlationId, result.CorrelationId);
            Assert.Equal(_fixture.ApplicationName, result.ApplicationName);
            Assert.Equal(Environment.MachineName, result.SenderHostname);
            Assert.InRange(DateTime.UtcNow.Subtract(result.CreatedTime.UtcDateTime).Milliseconds, 0, 100);
            Assert.Equal(message, result.Message);
        }
    }
}
