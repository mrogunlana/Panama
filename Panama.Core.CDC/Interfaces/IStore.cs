using Panama.Core.CDC.Models;

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
        Task ChangePublishedStateToDelayed(int[] ids);
        Task ChangeReceivedStateToDelayed(int[] ids);
        Task ChangeMessageState(string tableName, InternalMessage message, MessageStatus status, object? transaction = null);
        Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null);
        Task ChangeReceivedState(InternalMessage message, MessageStatus state, object? transaction = null);
        Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null);
        Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null);
        Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table);
        Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry();
        Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry();
        Task GetDelayedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
        Task GetDelayedPublishedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
        Task GetDelayedReceivedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
    }
}
