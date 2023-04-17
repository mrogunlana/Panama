using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Canal.RabbitMQ.Models;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.RabbitMQ
{
    public class RabbitMQProcess : IBrokerProcess
    {
        private Task? _task;
        private bool _isHealthy = true;
        private CancellationTokenSource _cts = new();

        private readonly IStore _store;
        private readonly CanalOptions _canal;
        private readonly ILogger<RabbitMQProcess> _log;
        private readonly IBrokerFactory _factory;
        private readonly IInvokeFactory _invokers;
        private readonly ConsumerSubscriptions _subscriptions;
        private readonly IServiceProvider _provider;

        public RabbitMQProcess(
              IStore store
            , ILogger<RabbitMQProcess> log
            , RabbitMQFactory factory
            , IServiceProvider provider
            , IOptions<CanalOptions> canal
            , ReceivedInvokerFactory invokers
            , ConsumerSubscriptions subscriptions)
        {
            _log = log;
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

                    await _provider.GetRequiredService<IBus>()
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(external.GetCorrelationId())
                        .Invoker(_invokers.GetInvoker())
                        .Post(external
                            .ResetId()
                            .AddCreatedTime()
                            .ToInternal(_provider)
                            .SetStatus(result.Success
                                ? MessageStatus.Scheduled
                                : MessageStatus.Failed)
                            .SetExpiration(_provider, DateTime.UtcNow))
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
            var subscriptions = _subscriptions.GetSubscriptions(typeof(RabbitMQTarget));

            foreach (var subscription in subscriptions)
            {
                ICollection<string> topics;
                try
                {
                    using (var client = _factory.Create(subscription.Key))
                        topics = client.GetOrAddTopics(subscription.Value.Select(x => x.Topic));
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
                            using (var client = _factory.Create(subscription.Key))
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
