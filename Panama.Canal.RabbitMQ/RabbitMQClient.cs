using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Extensions;
using Panama.Canal.RabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Panama.Canal.RabbitMQ
{
    public class RabbitMQClient : IBrokerClient
    {
        private readonly object _sync = new();
        private readonly string _queue;
        private readonly IPooledObjectPolicy<IModel> _models;
        private readonly RabbitMQOptions _options;
        private readonly string _exchange;
        private IModel? _channel;
        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }

        public RabbitMQClient(
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

        private async Task OnReceived(object? sender, BasicDeliverEventArgs e)
        {
            var headers = e.GetHeaders();

            headers.Add(Canal.Models.Headers.Group, _queue);

            var message = new TransientMessage(headers, e.Body.ToArray());

            await OnCallback!(message, e.DeliveryTag);
        }

        private Task OnShutdown(object? sender, ShutdownEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task OnRegistered(object? sender, ConsumerEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task OnUnregistered(object? sender, ConsumerEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task OnCancelled(object? sender, ConsumerEventArgs e)
        {
            return Task.CompletedTask;
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
            if (_channel!.IsOpen)
                _channel.BasicAck((ulong)sender!, false);
        }

        public void Listen(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Connect();

            if (_channel == null)
                throw new InvalidOperationException("Channel cannot be located.");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += OnReceived;
            consumer.Shutdown += OnShutdown;
            consumer.Registered += OnRegistered;
            consumer.Unregistered += OnUnregistered;
            consumer.ConsumerCancelled += OnCancelled;

            if (_options.QosPrefetchCount != 0)
                _channel.BasicQos(0, _options.QosPrefetchCount, _options.QosGlobal);

            _channel.BasicConsume(_queue, false, consumer);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }

        public void Reject(object? sender)
        {
            if (_channel!.IsOpen && sender is ulong val)
                _channel.BasicReject(val, true);
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            if (topics == null)
                throw new ArgumentNullException(nameof(topics));

            if (_channel == null)
                throw new InvalidOperationException("Channel cannot be located.");

            Connect();

            foreach (var topic in topics)
                _channel.QueueBind(_queue, _exchange, topic);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}