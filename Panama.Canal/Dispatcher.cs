using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Interfaces;
using Quartz;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Panama.Canal
{
    [DisallowConcurrentExecution]
    public class Dispatcher : IHostedService, IDispatcher
    {
        private bool _off;
        private CancellationTokenSource? _cts;

        private readonly IStore _store;
        private readonly ILogger<Dispatcher> _log;
        private readonly IServiceProvider _provider;
        private readonly IOptions<CanalOptions> _options;
        private readonly CancellationTokenSource _delay = new();
        private IEnumerable<IInitialize> _initializers = default!;
        private readonly PriorityQueue<InternalMessage, DateTime> _scheduled;

        private DateTime _next = DateTime.MaxValue;
        private Channel<InternalMessage> _published = default!;
        private ConcurrentDictionary<string, Channel<InternalMessage>> _received = default!;

        public bool Online => !_cts?.IsCancellationRequested ?? false;

        public IInvoke Brokers { get; set; }
        public IInvoke Subscriptions { get; set; }

        public Dispatcher(
              IStore store
            , ILogger<Dispatcher> log
            , IServiceProvider provider
            , IOptions<CanalOptions> options)
        {
            var capacity = options.Value.ProducerThreads * 500;

            _log = log;
            _store = store;
            _options = options;
            _provider = provider;
            _scheduled = new PriorityQueue<InternalMessage, DateTime>();
            _received = new ConcurrentDictionary<string, Channel<InternalMessage>>(
                options.Value.ConsumerThreads, options.Value.ConsumerThreads * 2);
            _published = Channel.CreateBounded<InternalMessage>(
                new BoundedChannelOptions(capacity > 5000 ? 5000 : capacity)
                {
                    AllowSynchronousContinuations = true,
                    SingleReader = options.Value.ProducerThreads == 1,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.Wait
                });

            Brokers = _provider.GetRequiredService<BrokerInvoker>();
            Subscriptions = _provider.GetRequiredService<SubscriptionInvoker>();
        }

        private async Task Initialize()
        {
            foreach (var initialize in _initializers)
            {
                try
                {
                    _cts!.Token.ThrowIfCancellationRequested();

                    await initialize.Invoke(_cts!.Token);
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException) throw;

                    _log.LogError(ex, "Initializing the processors!");
                }
            }
        }

        private void Delayed()
        {
            try
            {
                if (_scheduled.Count == 0) return;

                var ids = _scheduled.UnorderedItems.Select(x => x.Element._Id).ToArray();
                _store.ChangePublishedStateToDelayed(ids).GetAwaiter().GetResult();
                _log.LogDebug("Scheduled messages stored as delayed successfully.");
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Scheduled messages stored as delayed failed.");
            }
        }

        private async Task Publish()
        {
            try
            {
                while (await _published.Reader.WaitToReadAsync(_cts!.Token).ConfigureAwait(false))
                    while (_published.Reader.TryRead(out var message))
                        try
                        {
                            var result = await Brokers.Invoke(new MessageContext(message,
                                    provider: _provider,
                                    token: _cts.Token))
                                .ConfigureAwait(false);

                            if (!result.Success)
                                _log.LogError($"An exception occurred while publishing a message, reason(s):{string.Join(",", result.Messages)}. message id:{message.Id}");
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex,
                                $"An exception occurred when sending a message to the broker. Id:{message.Id}");
                        }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        private async Task Dequeue()
        {
            _cts!.Token.Register(() => Delayed());

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    while (_scheduled.TryPeek(out _, out _next))
                    {
                        var delayTime = _next - DateTime.UtcNow;

                        if (delayTime > new TimeSpan(500000)) //50ms
                            await Task.Delay(delayTime, _delay.Token);

                        _cts.Token.ThrowIfCancellationRequested();

                        var result = await Brokers.Invoke(new MessageContext(_scheduled.Dequeue(),
                                    provider: _provider,
                                    token: _cts.Token))
                                .ConfigureAwait(false);

                        if (!result.Success)
                            _log.LogError($"An exception occurred while publishing a message, reason(s):{string.Join(",", result.Messages)}.");
                    }
                    _cts.Token.WaitHandle.WaitOne(100);
                }
                catch (OperationCanceledException)
                {
                    //Ignore
                }
            }
        }

        private async Task Receive(string group, Channel<InternalMessage> channel)
        {
            try
            {
                while (await channel.Reader.WaitToReadAsync(_cts!.Token).ConfigureAwait(false))
                    while (channel.Reader.TryRead(out var message))
                        try
                        {
                            if (_log.IsEnabled(LogLevel.Debug))
                                _log.LogDebug($"Dispatching message for group {group}");

                            await Subscriptions
                                .Invoke(new MessageContext(message,
                                    provider: _provider,
                                    token: _cts.Token))
                                .ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //expected
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e,
                                $"An exception occurred when invoke subscriber. MessageId:{message.Id}");
                        }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        private Channel<InternalMessage> GetOrAddReceiver(string key)
        {
            return _received.GetOrAdd(key, group =>
            {
                _log.LogInformation($"Creating receiver channel for group {group} with thread count {1}");

                var capacity = 300;
                var channel = Channel.CreateBounded<InternalMessage>(
                    new BoundedChannelOptions(capacity > 3000 ? 3000 : capacity)
                    {
                        AllowSynchronousContinuations = true,
                        SingleReader = true,
                        SingleWriter = true,
                        FullMode = BoundedChannelFullMode.Wait
                    });

                Task.WhenAll(Enumerable.Range(0, 1)
                    .Select(_ => Task.Factory.StartNew(() => Receive(group, channel), _cts!.Token,
                        TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray());

                return channel;
            });
        }

        public Task Off(CancellationToken cancellationToken)
        {
            if (_off)
                return Task.CompletedTask;

            _cts?.Cancel();

            _cts?.Dispose();
            _cts = null;
            _off = true;

            return Task.CompletedTask;
        }

        public async Task On(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cts != null)
            {
                _log.LogInformation("### Panama Canal Dispatcher is already started!");

                return;
            }

            _log.LogDebug("### Panama Canal Dispatcher is starting.");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _cts.Token.Register(() => _delay.Cancel());
            _initializers = _provider.GetServices<IInitialize>();

            await Initialize().ConfigureAwait(false);

            await Task.WhenAll(Enumerable.Range(0, _options.Value.ProducerThreads)
                .Select(_ => Task.Factory.StartNew(Publish, _cts.Token,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray()).ConfigureAwait(false);

            GetOrAddReceiver(_options.Value.DefaultGroup);

            _ = Task.Factory.StartNew(Dequeue, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);

            _off = false;
            _log.LogInformation("### Panama Canal Dispatcher started!");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await On(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Off(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask Publish(InternalMessage message, CancellationToken? token = null)
        {
            try
            {
                if (!_published.Writer.TryWrite(message))
                    while (await _published.Writer.WaitToWriteAsync(_cts!.Token).ConfigureAwait(false))
                        if (_published.Writer.TryWrite(message))
                            return;
            }
            catch (OperationCanceledException)
            {
                //Ignore
            }
        }

        public async ValueTask Execute(InternalMessage message, CancellationToken? token = null)
        {
            try
            {
                var data = message.GetData<Message>(_provider);
                var group = data.GetGroup();

                var channel = GetOrAddReceiver(group);

                if (!channel.Writer.TryWrite(message))
                    while (await channel.Writer.WaitToWriteAsync(_cts!.Token).ConfigureAwait(false))
                        if (channel.Writer.TryWrite(message))
                            return;
            }
            catch (OperationCanceledException)
            {
                //Ignore
            }
        }

        public async ValueTask Schedule(InternalMessage message, DateTime delay, object? transaction = null, CancellationToken? token = null)
        {
            message.Expires = delay;

            var timeSpan = delay - DateTime.Now;

            if (timeSpan <= TimeSpan.FromMinutes(1))
            {
                await _store.ChangePublishedState(message, MessageStatus.Queued, transaction);

                _scheduled.Enqueue(message, delay);

                if (delay < _next)
                    _delay.Cancel();
            }
            else
            {
                await _store.ChangePublishedState(message, MessageStatus.Delayed, transaction);
            }
        }
    }
}
