using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.Sagas.Models;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Collections.Concurrent;

namespace Panama.Canal
{
    public class Store : IStore
    {
        private readonly ILogger<Store> _log;
        private readonly StoreOptions _options;
        private readonly CanalOptions _canalOptions;
        private readonly IStringEncryptor _encryptor;
        public ConcurrentDictionary<string, InternalMessage> Published { get; }
        public ConcurrentDictionary<string, InternalMessage> Received { get; }
        public ConcurrentDictionary<string, InternalMessage> Inbox { get; }
        public ConcurrentDictionary<string, InternalMessage> Outbox { get; }
        public ConcurrentDictionary<string, SagaEvent> Saga { get; }

        public Store(
              ILogger<Store> log
            , IOptions<StoreOptions> store
            , IOptions<CanalOptions> canal
            , StringEncryptorResolver stringEncryptorResolver)
        {
            _log = log;
            _options = store.Value;
            _canalOptions = canal.Value;
            _encryptor = stringEncryptorResolver(StringEncryptorResolverKey.Base64); ;

            Published = new ConcurrentDictionary<string, InternalMessage>();
            Received = new ConcurrentDictionary<string, InternalMessage>();
            Inbox = new ConcurrentDictionary<string, InternalMessage>();
            Outbox = new ConcurrentDictionary<string, InternalMessage>();
            Saga = new ConcurrentDictionary<string, SagaEvent>();
        }

        public Task<bool> AcquireLock(string key, TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AcquirePublishedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AcquireReceivedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            return Task.FromResult(true);
        }

        public Task ChangeMessageState(string tableName, InternalMessage message, MessageStatus status, object? transaction = null)
        {
            Published[message.Id].Status = status.ToString();

            return Task.CompletedTask;
        }

        public Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            Published[message.Id].Status = status.ToString();
            Published[message.Id].Expires = message.Expires ?? Published[message.Id].Expires;

            return Task.CompletedTask;
        }

        public Task ChangePublishedStateToDelayed(string[] ids)
        {
            var messages = Published.Where(x => ids.Contains(x.Value.Id)).Select(x => x.Value); 
            foreach (var message in messages)
                message.Status = MessageStatus.Delayed.ToString();

            return Task.CompletedTask;
        }

        public Task ChangeReceivedState(InternalMessage message, MessageStatus state, object? transaction = null)
        {
            Received[message.Id].Status = state.ToString();
            Received[message.Id].Expires = message.Expires ?? Received[message.Id].Expires;

            return Task.CompletedTask;
        }

        public Task ChangeReceivedStateToDelayed(string[] ids)
        {
            var messages = Received.Where(x => ids.Contains(x.Value.Id)).Select(x => x.Value); ;
            foreach (var message in messages)
                message.Status = MessageStatus.Delayed.ToString();

            return Task.CompletedTask;
        }

        public Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            if (table == nameof(_options.PublishedTable))
            {
                var ids = Published.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

                foreach (var id in ids)
                    if (Published.TryRemove(id, out _))
                        removed++;
            }
            else
            {
                var ids = Received.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

                foreach (var id in ids)
                    if (Received.TryRemove(id, out _))
                        removed++;
            }

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredInboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = Inbox.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (Inbox.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredOutboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = Outbox.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (Outbox.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = Published.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (Published.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = Received.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (Received.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredSagaEvents(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = Saga.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (Saga.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task GetDelayedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = Published.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task GetDelayedPublishedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = Published.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task GetDelayedReceivedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = Received.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.UtcNow.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table)
        {
            if (table == _options.PublishedTable)
            {
                return Task.FromResult(Published.Values
                    .Where(x => x.Retries < _canalOptions.FailedRetryCount
                                && x.Created < DateTime.UtcNow.AddSeconds(-10)
                                && (x.Status == MessageStatus.Scheduled.ToString() ||
                                    x.Status == MessageStatus.Failed.ToString()))
                    .Take(200)
                    .Select(x => x).AsEnumerable());
            }
            else
            {
                return Task.FromResult(Received.Values
                    .Where(x => x.Retries < _canalOptions.FailedRetryCount
                                && x.Created < DateTime.UtcNow.AddSeconds(-10)
                                && (x.Status == MessageStatus.Scheduled.ToString() ||
                                    x.Status == MessageStatus.Failed.ToString()))
                    .Take(200)
                    .Select(x => x).AsEnumerable());
            }
        }

        public Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry()
        {
            var result = Published.Values
                .Where(x => x.Retries < _canalOptions.FailedRetryCount
                            && x.Created < DateTime.UtcNow.AddSeconds(-10)
                            && (x.Status == MessageStatus.Scheduled.ToString() ||
                                x.Status == MessageStatus.Failed.ToString()))
                .Take(200)
                .Select(x => x).AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry()
        {
            var result = Received.Values
                .Where(x => x.Retries < _canalOptions.FailedRetryCount
                            && x.Created < DateTime.UtcNow.AddSeconds(-10)
                            && (x.Status == MessageStatus.Scheduled.ToString() ||
                                x.Status == MessageStatus.Failed.ToString()))
                .Take(200)
                .Select(x => x).AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<SagaEvent>> GetSagaEvents(string id)
        {
            var result = Saga.Values.Where(x => x.Id == id)
                .Select(x => x);

            return Task.FromResult(result);
        }

        public Task<Dictionary<int, string>> GetSchema(string table)
        {
            return Task.FromResult(new Dictionary<int, string>());
        }

        public Task<int> GetTableId(string table)
        {
            return Task.FromResult(1);
        }

        public Task Init()
        {
            return Task.CompletedTask;
        }

        public Task ReleaseLock(string key, string? instance = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task ReleasePublishedLock(string? instance = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task ReleaseReceivedLock(string? instance = null, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        public Task<InternalMessage> StoreInboxMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                 message.Content = _encryptor.ToString(message.Content);

            Inbox[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StoreOutboxMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            Outbox[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            Published[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            Received[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<SagaEvent> StoreSagaEvent(SagaEvent saga)
        {
            Saga[Guid.NewGuid().ToString()] = saga;

            return Task.FromResult(saga);
        }
    }
}
