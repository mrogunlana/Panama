using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Exceptions;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Models;
using Quartz;

namespace Panama.Canal.RabbitMQ.Jobs
{
    [DisallowConcurrentExecution]
    public class Default : IJob, IBrokerProcess
    {
        private Task? _task;
        private bool _isHealthy = true;
        private CancellationTokenSource _cts = new();

        private readonly IStore _store;
        private readonly CanalOptions _canal;
        private readonly ILogger<Default> _log;
        private readonly IBrokerFactory _factory;
        private readonly Subscriptions _subscriptions;
        private readonly IServiceProvider _provider;

        public Default(
              IStore store
            , ILogger<Default> log
            , IBrokerFactory factory
            , IServiceProvider provider
            , Subscriptions subscriptions
            , IOptions<CanalOptions> canal
            , IOptions<RabbitMQOptions> options)
        {
            _log = log;
            _store = store; 
            _provider = provider;
            _factory = factory;
            _canal = canal.Value;
            _subscriptions = subscriptions;
        }

        private void Pulse()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private void Register(IBrokerClient client)
        {
            client.OnCallback = async (message, sender) => {

                try
                {
                    var local = message.ToInternal(_provider);
                    if (local == null)
                        throw new InvalidOperationException($"Message id: {message.Headers[Canal.Models.Headers.Id]} could not be located.");

                    var metadata = local.GetData<Message>(_provider);

                    _log.LogInformation($"Received message. id:{local.Id}, name: {local.Name}");

                    metadata.RemoveException();

                    var subscription = _subscriptions.GetSubscription(typeof(RabbitMQTarget), metadata.GetGroup(), metadata.GetName());
                    if (subscription == null)
                        metadata.AddException(new SubscriptionException("Subscription could not be located."));

                    if (metadata.HasException())
                    {
                        await _store.StoreReceivedMessage(metadata
                            .ResetId()
                            .AddCreatedTime()
                            .ToInternal(_provider)
                            .SetStatus(MessageStatus.Failed)
                            .SetExpiration(_provider, DateTime.UtcNow));

                        client.Commit(sender);
                    }
                    else
                    {
                        await _store.StoreReceivedMessage(metadata
                            .ResetId()
                            .AddCreatedTime()
                            .ToInternal(_provider)
                            .SetStatus(MessageStatus.Scheduled)
                            .SetExpiration(_provider));

                        client.Commit(sender);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Exception occurred processing received message id: {message.Headers[Canal.Models.Headers.Id]}");

                    client.Reject(sender);
                }
            };
        }

        private async Task Execute()
        {
            var subscriptions = _subscriptions.GetSubscriptions(typeof(RabbitMQTarget));
            var tasks = new List<Task>();

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
                    return;
                }

                for (var i = 0; i < _canal.ConsumerThreads; i++)
                {
                    tasks.Add(Task.Factory.StartNew(() => {
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
                    }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default));
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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

        public Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, CancellationToken.None);
            _cts.Token.Register(() => {
                Pulse();
            });

            Task.WaitAll(Execute());

            return Task.CompletedTask;
        }
    }
}
