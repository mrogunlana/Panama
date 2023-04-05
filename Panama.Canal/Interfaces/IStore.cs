using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IStore
    {
        Task Init();
        Task<Dictionary<int, string>> GetSchema(string table);
        Task<int> GetTableId(string table);
        Task<bool> AcquireLock(string key, TimeSpan ttl, string? instance = null, CancellationToken token = default);
        Task<bool> AcquirePublishedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default);
        Task<bool> AcquireReceivedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default);
        Task ReleaseLock(string key, string? instance = null, CancellationToken token = default);
        Task ReleasePublishedLock(string? instance = null, CancellationToken token = default);
        Task ReleaseReceivedLock(string? instance = null, CancellationToken token = default);
        Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default);
        Task ChangePublishedStateToDelayed(int[] ids);
        Task ChangeReceivedStateToDelayed(int[] ids);
        Task ChangeMessageState(string tableName, InternalMessage message, MessageStatus status, object? transaction = null);
        Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null);
        Task ChangeReceivedState(InternalMessage message, MessageStatus state, object? transaction = null);
        Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null);
        Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null);
        Task<InternalMessage> StoreInboxMessage(InternalMessage message, object? transaction = null);
        Task<InternalMessage> StoreOutboxMessage(InternalMessage message, object? transaction = null);
        Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredInboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<int> DeleteExpiredOutboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default);
        Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table);
        Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry();
        Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry();
        Task GetDelayedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
        Task GetDelayedPublishedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
        Task GetDelayedReceivedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default);
        Task<SagaEvent> StoreSagaEvent(SagaEvent saga);
        Task<IEnumerable<SagaEvent>> GetSagaEvents(string id);
        Task<int> DeleteExpiredSagaEvents(DateTime timeout, int batch = 1000, CancellationToken token = default);
    }
}
