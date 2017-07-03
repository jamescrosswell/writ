using System;
using Xunit;

namespace Messaging.Tests
{
    public class DefaultCorrelationProviderTests
    {
        [Fact]
        public void GetCurrentCorrelationId_ReturnsNonNull()
        {
            var correlationProvider = new DefaultCorrelationProvider();
            var result = correlationProvider.GetCurrentCorrelationId();
            Assert.NotNull(result);
        }

        [Fact]
        public void NewContext_GeneratesNewCorrelationId()
        {
            var correlationProvider = new DefaultCorrelationProvider();
            var first = correlationProvider.GetCurrentCorrelationId();

            var second = correlationProvider.NewContext();

            Assert.NotEqual(first, second);
        }
    }
}
