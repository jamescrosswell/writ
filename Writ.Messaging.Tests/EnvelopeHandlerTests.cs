using System;
using NSubstitute;
using Xunit;

namespace Writ.Messaging.Tests
{
    public class EnvelopeHandlerTests
    {
        private sealed class Fixture
        {
            public string ApplicationName { get; } = "Messaging.Tests";

            public EnvelopeHandler GetSut(CorrelationProvider correlationProvider)
            {
                return new EnvelopeHandler(ApplicationName, correlationProvider);
            }
        }    

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void Stuff_ReturnsMessageEnvelope()
        {
            var message = new Object();
            var correlationId = Guid.NewGuid().ToString();
            var sut = _fixture.GetSut(() => correlationId);

            var result = sut.Stuff(message);

            Assert.Equal(correlationId, result.CorrelationId);
            Assert.Equal(_fixture.ApplicationName, result.ApplicationName);
            Assert.Equal(Environment.MachineName, result.SenderHostname);
            Assert.Equal(message, result.Message);
        }
    }
}
