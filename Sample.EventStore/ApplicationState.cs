using LiteDB;
using Sample.Domain.Accounts;
using System.IO;
using Sample.Domain;

namespace Sample.EventStore
{
    public class ApplicationState
    {
        private readonly LiteDatabase _store = new LiteDatabase(new MemoryStream());
        public LiteCollection<Account> Accounts => _store.GetCollection<Account>();
        public LiteCollection<HighWaterMark> HighWaterMarks => _store.GetCollection<HighWaterMark>();

        public LiteTransaction BeginTrans()
        {
            return _store.BeginTrans();
        }
    }
}
