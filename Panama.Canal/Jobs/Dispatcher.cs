using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class Dispatcher : IDispatcher, IJob
    {
        private CancellationTokenSource? _cts;

        private readonly IStore _store;
        private readonly IInvokeBrokers _brokers;
        private readonly ILogger<Dispatcher> _log;
        private readonly IOptions<CanalOptions> _options;
        private readonly IInvokeSubscriptions _subscriptions;
        private readonly CancellationTokenSource _delay = new();
        private readonly PriorityQueue<InternalMessage, DateTime> _scheduled;

        private Channel<InternalMessage> _published = default!;
        private ConcurrentDictionary<string, Channel<(InternalMessage, SubscriptionDescriptor?)>> _received = default!;

        public Dispatcher(
              IStore store
            , IInvokeBrokers brokers
            , ILogger<Dispatcher> log
            , IInvokeSubscriptions subscriptions
            , IOptions<CanalOptions> options)
        {
            var capacity = 500;

            _log = log;
            _store = store;
            _brokers = brokers;
            _options = options;
            _subscriptions = subscriptions;
            _scheduled = new PriorityQueue<InternalMessage, DateTime>();
            _received = new ConcurrentDictionary<string, Channel<(InternalMessage, SubscriptionDescriptor?)>>(1, 2);
            _published = Channel.CreateBounded<InternalMessage>(
                new BoundedChannelOptions(capacity > 5000 ? 5000 : capacity) {
                    AllowSynchronousContinuations = true,
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.Wait
                });
        }

        public async Task Publish()
        {
            try
            {
                while (await _published.Reader.WaitToReadAsync(_cts!.Token).ConfigureAwait(false))
                    while (_published.Reader.TryRead(out var message))
                        try
                        {
                            //var result = await _sender.SendAsync(message).ConfigureAwait(false);
                            //if (!result.Succeeded)
                            //    _logger.MessagePublishException(message.Origin.GetId(), result.ToString(), result.Exception);
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

        public async Task Received(string group, Channel<(InternalMessage, SubscriptionDescriptor?)> channel)
        {
            try
            {
                while (await channel.Reader.WaitToReadAsync(_cts!.Token).ConfigureAwait(false))
                    while (channel.Reader.TryRead(out var message))
                        try
                        {
                            if (_log.IsEnabled(LogLevel.Debug))
                                _log.LogDebug("Dispatching message for group {ConsumerGroup}", group);

                            //await _executor.ExecuteAsync(message.Item1, message.Item2, _tasksCts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            //expected
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e,
                                $"An exception occurred when invoke subscriber. MessageId:{message.Item1.Id}");
                        }
            }
            catch (OperationCanceledException)
            {
                // expected
            }
        }

        private Channel<(InternalMessage, SubscriptionDescriptor?)> GetOrAddReceiver(string key)
        {
            return _received.GetOrAdd(key, group =>
            {
                _log.LogInformation($"Creating receiver channel for group {group} with thread count {1}");

                var capacity = 300;
                var channel = Channel.CreateBounded<(InternalMessage, SubscriptionDescriptor?)>(
                    new BoundedChannelOptions(capacity > 3000 ? 3000 : capacity) {
                        AllowSynchronousContinuations = true,
                        SingleReader = true,
                        SingleWriter = true,
                        FullMode = BoundedChannelFullMode.Wait
                    });

                Task.WhenAll(Enumerable.Range(0, 1)
                    .Select(_ => Task.Factory.StartNew(() => Received(group, channel), _cts!.Token,
                        TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray());

                return channel;
            });
        }

        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, CancellationToken.None);
            _cts.Token.Register(() => _delay.Cancel());

            var results = new List<Task>();

            var tasks = await Task.WhenAll(Enumerable.Range(0, 1)
            .Select(_ => Task.Factory.StartNew(Publish, context.CancellationToken,
                TaskCreationOptions.LongRunning, TaskScheduler.Default)).ToArray());

            results.AddRange(tasks);

            GetOrAddReceiver(_options.Value.DefaultGroupName);

            var task = await Task.Factory.StartNew(async () => {

                _cts.Token.Register(() => {
                    try
                    {
                        if (_scheduled.Count == 0) return;

                        var ids = _scheduled.UnorderedItems.Select(x => x.Element._Id).ToArray();
                        _store.ChangePublishedStateToDelayed(ids).GetAwaiter().GetResult();
                        _log.LogDebug("Update storage to delayed success of delayed message in memory queue!");
                    }
                    catch (Exception e)
                    {
                        _log.LogWarning(e, "Update storage fails of delayed message in memory queue!");
                    }
                });

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var next = DateTime.MaxValue;

                        while (_scheduled.TryPeek(out _, out next))
                        {
                            var delayTime = next - DateTime.Now;

                            if (delayTime > new TimeSpan(500000)) //50ms
                                await Task.Delay(delayTime, _cts.Token);

                            _cts.Token.ThrowIfCancellationRequested();

                            //await _sender.SendAsync(_scheduled.Dequeue()).ConfigureAwait(false);
                        }
                        _cts.Token.WaitHandle.WaitOne(100);
                    }
                    catch (OperationCanceledException)
                    {
                        //Ignore
                    }
                }

            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);

            results.Add(task);

            Task.WaitAll(tasks);
        }
        
        public ValueTask Publish(InternalMessage message, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask Execute(InternalMessage message, object? descriptor = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public ValueTask Schedule(InternalMessage message, DateTime delay, object? transaction = null, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }
    }
}
