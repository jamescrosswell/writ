using System;
using Writ.Messaging.Kafka.Events;

namespace Sample.Domain.Accounts
{
    public class Account: IAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public string AccountHolder { get; set; }
        public int Balance { get; set; }
    }
}