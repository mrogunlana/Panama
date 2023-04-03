using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Interfaces;
using Panama.Models;
using System.Collections.Concurrent;

namespace Panama.Canal.Channels
{
    public class DefaultChannel : IChannel
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _provider;
        private readonly IDispatcher _dispatcher;

        public EventContext Context { get; }
        public IInvoke Invoker { get; set; }
        public ConcurrentQueue<InternalMessage> Queue { get; }
        public DefaultChannel(
              ILogger log
            , IDispatcher dispatcher
            , IServiceProvider provider)
        {
            _log = log;
            _provider = provider;
            _dispatcher = dispatcher;
            
            Queue = new ConcurrentQueue<InternalMessage>();
            Context = new EventContext(provider);
            Invoker = _provider.GetRequiredService<StreamInvoker>();
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

        public virtual async Task Post(string name, string? ack = null, string? nack = null, string? group = null, DateTime? delay = null, string? instance = null, string? correlationId = null, CancellationToken token = default, IDictionary<string, string?>? headers = null, params IModel[]? data)
        {
            await Post<DefaultTarget, StreamInvoker>(
                name: name,
                ack: ack,
                nack: nack,
                group: group,
                delay: delay,
                instance: instance,
                correlationId: correlationId,
                data: data,
                headers: headers).ConfigureAwait(false);
        }

        public virtual async Task Post<T>(string name, string? ack = null, string? nack = null, string? group = null, DateTime? delay = null, string? instance = null, string? correlationId = null, CancellationToken token = default, IDictionary<string, string?>? headers = null, params IModel[]? data) where T : ITarget
        {
            await Post<T, StreamInvoker>(
                name: name,
                ack: ack,
                nack: nack,
                group: group,
                delay: delay,
                instance: instance,
                correlationId: correlationId,
                data: data,
                headers: headers).ConfigureAwait(false);
        }

        public virtual async Task Post<T, I>(string name, string? ack = null, string? nack = null, string? group = null, DateTime? delay = null, string? instance = null, string? correlationId = null, CancellationToken token = default, IDictionary<string, string?>? headers = null, params IModel[]? data) 
            where T : ITarget
            where I : IInvoke
        {
            token.ThrowIfCancellationRequested();

            var result = await _provider.GetRequiredService<IBus>()
                .Instance(instance)
                .Header(headers)
                .Token(token)
                .Id(Guid.NewGuid().ToString())
                .CorrelationId(correlationId)
                .Topic(name)
                .Group(group)
                .Data(data)
                .Ack(ack)
                .Nack(nack)
                .Invoker<I>()
                .Target<T>()
                .Delay(delay)
                .Post();

            Queue.EnqueueResult(result);
        }

        public virtual void Rollback(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
        }
    }
}
