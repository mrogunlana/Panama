using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Models;
using Quartz;
using RabbitMQ.Client;

namespace Panama.Canal.RabbitMQ.Jobs
{
    [DisallowConcurrentExecution]
    public class Default : IJob, IBrokerClient
    {
        private CancellationTokenSource? _cts;

        private readonly object _sync = new();
        private readonly string _queue;
        private readonly IPooledObjectPolicy<IModel> _models;
        private readonly RabbitMQOptions _options;
        private readonly string _exchange;
        private IModel? _channel;

        public Func<InternalMessage, object?, Task>? Callback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Default(
              string queue
            , IOptions<CanalOptions> canal
            , IOptions<RabbitMQOptions> options
            , IPooledObjectPolicy<IModel> models)
        {
            _queue = queue;
            _models = models;
            _options = options.Value;
            _exchange = $"{options.Value.Exchange}.{canal.Value.Version}";
        }

        public void Connect()
        {
            var connection = _models.Create();

            lock (_sync)
            {
                if (_channel == null || _channel.IsClosed)
                {
                    _channel = _models.Create();
                    _channel.ExchangeDeclare(_exchange, "topic", true);

                    var arguments = new Dictionary<string, object> {
                        {"x-message-ttl", _options.MessageTTL}
                    };

                    if (!string.IsNullOrEmpty(_options.QueueMode))
                        arguments.Add("x-queue-mode", _options.QueueMode);

                    _channel.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false, arguments: arguments);
                }
            }
        }

        public void Commit(object? sender)
        {
            throw new NotImplementedException();
        }

        public Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, CancellationToken.None);
            _cts.Token.Register(() => { 
                //TODO: disconnect broker model 
            });

            throw new NotImplementedException();
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Reject(object? sender)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            throw new NotImplementedException();
        }
    }
}
