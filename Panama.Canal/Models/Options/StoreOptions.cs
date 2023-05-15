using Panama.Canal.Interfaces;

namespace Panama.Canal.Models.Options
{
    public class StoreOptions : IStoreOptions
    {
        public string PublishedTable { set; get; } = "Published";
        public string ReceivedTable { set; get; } = "Received";
        public string OutboxTable { set; get; } = "Outbox";
        public string InboxTable { set; get; } = "Inbox";
        public string LockTable { set; get; } = "Lock";
        public ProcessingType ProcessingType { get; set; }

        public StoreOptions()
        {
            ProcessingType = ProcessingType.Stream;
        }
    }
}
