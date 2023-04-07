using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class MemorySettings : IModel
    {
        public string PublishedTable { set; get; } = "Published";
        public string ReceivedTable { set; get; } = "Received";
        public string OutboxTable { set; get; } = "Outbox";
        public string InboxTable { set; get; } = "Inbox";
        public string LockTable { set; get; } = "Lock";
    }
}
