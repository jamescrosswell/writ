using System;
using Sample.Domain.Accounts;
using Writ.Messaging.Kafka.Events;
using Xunit;

namespace Sample.Domain.Tests.Accounts
{
    public class AccountCreatedTests
    {
        [Fact]
        public void AccountCreated_Equals()
        {
            var offsetDee = new MessageOffset("foo", 7, 101);
            var tweedleDee = new AccountCreated(offsetDee, Guid.NewGuid(), "Emily Downs");

            var offsetDum = new MessageOffset(offsetDee.Topic, offsetDee.Partition, offsetDee.Offset);
            var tweedleDum = new AccountCreated(offsetDum, tweedleDee.Id, tweedleDee.AccountHolder);

            Assert.Equal(offsetDee, offsetDum);
            Assert.Equal(tweedleDee, tweedleDum);

            var wrongOffsetTopic = new AccountCreated(new MessageOffset("bar", 7, 101), Guid.NewGuid(), "Emily Downs");
            var wrongOffsetPartition = new AccountCreated(new MessageOffset("foo", 77, 101), Guid.NewGuid(), "Emily Downs");
            var wrongOffsetOffset = new AccountCreated(new MessageOffset("foo", 7, 401), Guid.NewGuid(), "Emily Downs");
            var wrongGuid = new AccountCreated(offsetDee, Guid.NewGuid(), "Emily Downs");
            var wrongAccountHolder = new AccountCreated(offsetDee, Guid.NewGuid(), "Emily Downs");

            Assert.NotEqual(tweedleDee, wrongOffsetTopic);
            Assert.NotEqual(tweedleDee, wrongOffsetPartition);
            Assert.NotEqual(tweedleDee, wrongOffsetOffset);
            Assert.NotEqual(tweedleDee, wrongGuid);
            Assert.NotEqual(tweedleDee, wrongAccountHolder);
        }
    }
}
