using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Collections.Concurrent;
using System.Data;

namespace Panama.Canal
{
    public class CanalTransaction : ITransaction
    {
        private readonly ConcurrentQueue<InternalMessage> _queue;
        private readonly IDispatcher _dispatcher;

        private readonly ILogger<Store> _log;
        private readonly IServiceProvider _provider;

        public CanalTransaction(
              ILogger<Store> log
            , IDispatcher dispatcher
            , IServiceProvider provider)
        {
            _log = log;
            _provider = provider;
            _dispatcher = dispatcher;

            _queue = new ConcurrentQueue<InternalMessage>();
        }

        public bool AutoCommit { get; set; }

        public virtual object? DbTransaction { get; set; }

        public Task Commit(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Rollback(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Queue(InternalMessage message)
        {
            _queue.Enqueue(message);
        }

        public async Task Flush(CancellationToken token = default)
        {
            while (!_queue.IsEmpty)
            {
                if (_queue.TryDequeue(out var message))
                {
                    var data = message.GetData<Message>(_provider);
                    var delay = data.GetDelay();

                    if (delay == DateTime.MinValue)
                        await _dispatcher.Publish(
                            message: message,
                            token: token)
                            .ConfigureAwait(false);
                    else
                        await _dispatcher.Schedule(
                            message: message,
                            delay: delay,
                            token: token)
                            .ConfigureAwait(false);
                }
            }
        }
    }
}