namespace Panama.Core.CDC.Interfaces
{
    public interface IStore
    {
        Task InitLocks();
        Dictionary<int, string> GetSchema(object tableId);
        Task<bool> AcquireLock(string key, TimeSpan ttl, string instance, CancellationToken token = default);
        Task ReleaseLock(string key, string instance, CancellationToken token = default);
        Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default);
        Task ChangeMessageState(string key, TimeSpan ttl, string instance, CancellationToken token = default);
    }
}
