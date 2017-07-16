using LiteDB;
using Sample.Domain.Accounts;
using System.IO;

namespace Sample.EventStore
{
    public class ApplicationState
    {
        private readonly LiteDatabase _store = new LiteDatabase(new MemoryStream());

        public ApplicationState()
        {
            var memoryStore = new MemoryStream();
            _store = new LiteDatabase(memoryStore);
        }

        public LiteCollection<Account> Accounts => _store.GetCollection<Account>();
    }
}
