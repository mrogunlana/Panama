using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IStoreOptions
    {
        string PublishedTable { set; get; }
        string ReceivedTable { set; get; }
        string OutboxTable { set; get; }
        string InboxTable { set; get; }
        string LockTable { set; get; }
        ProcessingType ProcessingType { get; set; }
    }
}