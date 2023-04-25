using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.Models.Descriptors;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Brokers
{
    public class BrokerProcess : IBrokerProcess
    {
        private Task? _task;
        private bool _isHealthy = true;
        private CancellationTokenSource _cts = new();

        private readonly IStore _store;
        private readonly CanalOptions _canal;
        private readonly ILogger<BrokerProcess> _log;
        private readonly IBrokerFactory _factory;
        private readonly IInvokeFactory _invokers;
        private readonly SubscriberDescriptions _subscriptions;
        private readonly SagaDescriptions _sagas;
        private readonly IServiceProvider _provider;

        public BrokerProcess(
              IStore store
            , ILogger<BrokerProcess> log
            , BrokerFactory factory
            , SagaDescriptions sagas
            , IServiceProvider provider
            , IOptions<CanalOptions> canal
            , ReceivedInvokerFactory invokers
            , SubscriberDescriptions subscriptions)
        {
            _log = log;
            _sagas = sagas;
            _store = store;
            _factory = factory;
            _provider = provider;
            _canal = canal.Value;
            _invokers = invokers;
            _subscriptions = subscriptions;
        }

        private void Pulse()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private void Register(IBrokerClient client)
        {
            client.OnCallback = async (message, sender) =>
            {

                try
                {
                    _log.LogInformation($"Received message. ID:{message.GetId()}.");

                    var result = message.TryGetModels(_provider);
                    var transient = result.DataGetSingle<TransientMessage>();
                    var external = result.DataGetSingle<Message>();
                    var local = result.DataGetSingle<InternalMessage>();

                    //received id = published id
                    var value = external
                        .AddCreatedTime()
                        .ToInternal(_provider)
                        .SetStatus(MessageStatus.Scheduled);

                    if (!result.Success)
                        value = value
                            .SetStatus(MessageStatus.Failed)
                            .SetFailedExpiration(_provider, DateTime.UtcNow);

                    await new Context(_provider).Bus()
                        .CorrelationId(external.GetCorrelationId())
                        .Invoker(_invokers.GetInvoker())
                        .Post(value)
                        .ConfigureAwait(false);

                    client.Commit(sender);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Exception occurred processing received message id: {message.Headers[Headers.Id]}");

                    client.Reject(sender);
                }
            };
        }

        private Task Execute()
        {
            var subscriptions = _subscriptions.GetDescriptions(typeof(DefaultTarget));
            var sagas = _sagas.GetDescriptions(typeof(DefaultTarget));
            var descriptions = subscriptions.Concat(sagas).ToDictionary<DefaultTarget>();

            foreach (var description in descriptions)
            {
                var topics = new List<string>();
                try
                {
                    using (var client = _factory.Create(description.Key))
                        topics.AddRange(client.GetOrAddTopics(description.Value.Select(x => x.Topic)));
                }
                catch (BrokerException e)
                {
                    _isHealthy = false;
                    _log.LogError(e, e.Message);

                    return Task.CompletedTask;
                }

                for (var i = 0; i < _canal.ConsumerThreads; i++)
                {
                    _ = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            using (var client = _factory.Create(description.Key))
                            {
                                Register(client);

                                client.Subscribe(topics);

                                client.Listen(TimeSpan.FromSeconds(1), _cts.Token);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            //ignore
                        }
                        catch (BrokerException e)
                        {
                            _isHealthy = false;
                            _log.LogError(e, e.Message);
                        }
                        catch (Exception e)
                        {
                            _log.LogError(e, e.Message);
                        }
                    }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            }

            return Task.CompletedTask;
        }

        public bool IsHealthy()
        {
            return _isHealthy;
        }

        public void Restart(bool force = false)
        {
            if (!IsHealthy() || force)
            {
                Pulse();

                _cts = new CancellationTokenSource();
                _isHealthy = true;

                Execute().GetAwaiter().GetResult();
            }
        }

        public async Task Start(IContext context)
        {
            context.Token.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.Token, CancellationToken.None);
            _cts.Token.Register(() =>
            {
                Pulse();
            });

            await Execute();
        }
    }
}
