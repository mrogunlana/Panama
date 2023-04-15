using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Quartz;
using System.Text;

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
        private readonly IInvokeFactory _invokers;
        private readonly ConsumerSubscriptions _subscriptions;
        private readonly IServiceProvider _provider;
        
        public Default(
              IStore store
            , ILogger<Default> log
            , RabbitMQFactory factory
            , IServiceProvider provider
            , IOptions<CanalOptions> canal
            , ConsumerSubscriptions subscriptions
            , ReceivedInvokerFactory processor)
        {
            _log = log;
            _store = store;
            _factory = factory;
            _provider = provider;
            _canal = canal.Value;
            _invokers = processor;
            _subscriptions = subscriptions;
        }

        private void Pulse()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        private IResult TryGetModels(TransientMessage message)
        {
            try
            {
                message.RemoveException();

                var local = message.ToInternal(_provider);
                if (local == null)
                    throw new InvalidOperationException($"Internal Message ID: {message.Headers[Headers.Id]} could not be located.");

                var external = local.GetData<Message>(_provider);
                if (external == null)
                    throw new InvalidOperationException($"Message could not be located in Internal Message ID: {message.Headers[Headers.Id]} .");

                external.RemoveException();

                var result = _subscriptions.HasSubscribers(external);
                if (result == false)
                    throw new InvalidCastException($"No subscribers can be found for message ID: {message.Headers[Headers.Id]}.");

                return new Result()
                    .Success()
                    .Add(local)
                    .Add(external)
                    .Add(message);
            }
            catch (Exception ex)
            {
                message.AddException(ex);

                var external = new Message(message.Headers, Encoding.UTF8.GetString(message.Body.ToArray()))
                        .AddCreatedTime()
                        .AddException(ex)
                        .AddMessageId(Guid.NewGuid().ToString());
                
                return new Result()
                    .Fail()
                    .Add(message)
                    .Add(external)
                    .Add(external.ToInternal(_provider));
            }
        }
        
        private void Register(IBrokerClient client)
        {
            client.OnCallback = async (message, sender) => {

                try
                {
                    _log.LogInformation($"Received message. ID:{message.GetId()}.");

                    var result = TryGetModels(message);
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
