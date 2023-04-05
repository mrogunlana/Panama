using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Collections.Concurrent;

namespace Panama.Canal
{
    public class Store : IStore
    {
        private readonly ILogger<Store> _log;
        private readonly MemorySettings _settings;
        private readonly IOptions<CanalOptions> _canalOptions;
        private readonly IStringEncryptor _encryptor;

        private readonly ConcurrentDictionary<string, InternalMessage> _published;
        private readonly ConcurrentDictionary<string, InternalMessage> _received;
        private readonly ConcurrentDictionary<string, InternalMessage> _inbox;
        private readonly ConcurrentDictionary<string, InternalMessage> _outbox;
        private readonly ConcurrentDictionary<string, SagaEvent> _saga;


        public Store(
              ILogger<Store> log
            , MemorySettings settings
            , IOptions<CanalOptions> options
            , StringEncryptorResolver stringEncryptorResolver)
        {
            _log = log;
            _settings = settings;
            _canalOptions = options;
            _encryptor = stringEncryptorResolver(StringEncryptorResolverKey.Base64); ;

            _published = new ConcurrentDictionary<string, InternalMessage>();
            _received = new ConcurrentDictionary<string, InternalMessage>();
            _inbox = new ConcurrentDictionary<string, InternalMessage>();
            _outbox = new ConcurrentDictionary<string, InternalMessage>();
            _saga = new ConcurrentDictionary<string, SagaEvent>();
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
            _published[message.Id].Status = status.ToString();

            return Task.CompletedTask;
        }

        public Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            _published[message.Id].Status = status.ToString();

            return Task.CompletedTask;
        }

        public Task ChangePublishedStateToDelayed(int[] ids)
        {
            var messages = _published.Where(x => ids.Contains(x.Value._Id)).Select(x => x.Value); ;
            foreach (var message in messages)
                message.Status = MessageStatus.Delayed.ToString();

            return Task.CompletedTask;
        }

        public Task ChangeReceivedState(InternalMessage message, MessageStatus state, object? transaction = null)
        {
            _received[message.Id].Status = state.ToString();

            return Task.CompletedTask;
        }

        public Task ChangeReceivedStateToDelayed(int[] ids)
        {
            var messages = _received.Where(x => ids.Contains(x.Value._Id)).Select(x => x.Value); ;
            foreach (var message in messages)
                message.Status = MessageStatus.Delayed.ToString();

            return Task.CompletedTask;
        }

        public Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            if (table == nameof(_settings.PublishedTable))
            {
                var ids = _published.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

                foreach (var id in ids)
                    if (_published.TryRemove(id, out _))
                        removed++;
            }
            else
            {
                var ids = _received.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

                foreach (var id in ids)
                    if (_received.TryRemove(id, out _))
                        removed++;
            }

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredInboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = _inbox.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (_inbox.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredOutboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = _outbox.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (_outbox.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = _published.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (_published.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = _received.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (_received.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task<int> DeleteExpiredSagaEvents(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            var removed = 0;
            var ids = _saga.Values
                    .Where(x => x.Expires < timeout)
                    .Select(x => x.Id)
                    .Take(batch);

            foreach (var id in ids)
                if (_outbox.TryRemove(id, out _))
                    removed++;

            return Task.FromResult(removed);
        }

        public Task GetDelayedMessagesForScheduling(string table, Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = _published.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.Now.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.Now.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task GetDelayedPublishedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = _published.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.Now.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.Now.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task GetDelayedReceivedMessagesForScheduling(Func<object, IEnumerable<InternalMessage>, Task> task, CancellationToken token = default)
        {
            var result = _received.Values.Where(x =>
                    (x.Status == MessageStatus.Delayed.ToString() && x.Expires < DateTime.Now.AddMinutes(2)) ||
                    (x.Status == MessageStatus.Queued.ToString() && x.Expires < DateTime.Now.AddMinutes(-1)))
                .Select(x => x);

            return task(null!, result);
        }

        public Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table)
        {
            if (table == _settings.PublishedTable)
            {
                return Task.FromResult(_published.Values
                    .Where(x => x.Retries < _canalOptions.Value.FailedRetryCount
                                && x.Created < DateTime.Now.AddSeconds(-10)
                                && (x.Status == MessageStatus.Scheduled.ToString() ||
                                    x.Status == MessageStatus.Failed.ToString()))
                    .Take(200)
                    .Select(x => x).AsEnumerable());
            }
            else
            {
                return Task.FromResult(_received.Values
                    .Where(x => x.Retries < _canalOptions.Value.FailedRetryCount
                                && x.Created < DateTime.Now.AddSeconds(-10)
                                && (x.Status == MessageStatus.Scheduled.ToString() ||
                                    x.Status == MessageStatus.Failed.ToString()))
                    .Take(200)
                    .Select(x => x).AsEnumerable());
            }
        }

        public Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry()
        {
            var result = _published.Values
                .Where(x => x.Retries < _canalOptions.Value.FailedRetryCount
                            && x.Created < DateTime.Now.AddSeconds(-10)
                            && (x.Status == MessageStatus.Scheduled.ToString() ||
                                x.Status == MessageStatus.Failed.ToString()))
                .Take(200)
                .Select(x => x).AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry()
        {
            var result = _received.Values
                .Where(x => x.Retries < _canalOptions.Value.FailedRetryCount
                            && x.Created < DateTime.Now.AddSeconds(-10)
                            && (x.Status == MessageStatus.Scheduled.ToString() ||
                                x.Status == MessageStatus.Failed.ToString()))
                .Take(200)
                .Select(x => x).AsEnumerable();

            return Task.FromResult(result);
        }

        public Task<IEnumerable<SagaEvent>> GetSagaEvents(string id)
        {
            var result = _saga.Values.Where(x => x.Id == id)
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

            _inbox[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StoreOutboxMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            _outbox[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            _published[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null)
        {
            if (!message.IsContentBase64())
                message.Content = _encryptor.ToString(message.Content);

            _received[message.Id] = message;

            return Task.FromResult(message);
        }

        public Task<SagaEvent> StoreSagaEvent(SagaEvent saga)
        {
            _saga[saga.Id] = saga;

            return Task.FromResult(saga);
        }
    }
}
