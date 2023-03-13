namespace Panama.Canal.Models
{
    public enum MessageStatus 
    {
        None = 0,
        Queued = 10,
        Delayed = 15,
        Succeeded = 20,
        Failed = 30,
        Scheduled = 40
    }
}
