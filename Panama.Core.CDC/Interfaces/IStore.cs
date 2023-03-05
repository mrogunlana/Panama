namespace Panama.Core.CDC.Interfaces
{
    public interface IStore
    {
        Task Init();
        Task<Dictionary<int, string>> GetPublishedSchema();
        Task<Dictionary<int, string>> GetReceivedSchema();
        Task<int> GetReceivedTableId();
        Task<int> GetPublishedTableId();
        Task<bool> AcquireLock(string key, TimeSpan ttl, string instance, CancellationToken token = default);
        Task ReleaseLock(string key, string instance, CancellationToken token = default);
        Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default);
        Task ChangeMessageState(string tableName, _Message message, MessageStatus status, object? transaction = null);
        Task ChangePublishedState(_Message message, MessageStatus status, object? transaction = null);
        Task ChangeReceivedState(_Message message, MessageStatus state, object? transaction = null);
        Task<_Message> StorePublishedMessage(_Message message, object? transaction = null);
        Task<_Message> StoreReceivedMessage(_Message message, object? transaction = null);
        Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
    }
}
