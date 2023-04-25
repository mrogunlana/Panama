using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Canal.Channels
{
    public class DefaultChannel : IChannel
    {
        private readonly ILogger<DefaultChannel> _log;
        private readonly IProcessorFactory _factory;
        private readonly IServiceProvider _provider;
        
        public object? Current { get; set; }
        public IInvoke Invoker { get; set; }
        public ConcurrentQueue<InternalMessage> Queue { get; }
        public DefaultChannel(
              IProcessorFactory factory
            , IServiceProvider provider)
        {
            _factory = factory;
            _provider = provider;
            _log = _provider.GetRequiredService<ILogger<DefaultChannel>>();

            Current = new object();
            Queue = new ConcurrentQueue<InternalMessage>();
            Invoker = _provider.GetRequiredService<OutboxInvoker>();
        }

        public virtual async Task Commit(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await Flush().ConfigureAwait(false);
        }

        public virtual void Dispose()
        {
            Queue.Clear();
        }

        public virtual async Task Flush(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            while (!Queue.IsEmpty)
            {
                if (Queue.TryDequeue(out var message))
                {
                    token.ThrowIfCancellationRequested();

                    await _factory
                        .GetProducerProcessor(message)
                        .Execute(new Panama.Models.Context()
                            .Add(message)
                            .Token(token));
                }
            }
        }

        public virtual void Rollback(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
        }
    }
}
