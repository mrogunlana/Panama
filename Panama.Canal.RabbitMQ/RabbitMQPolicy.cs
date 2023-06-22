using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Models.Options;
using Panama.Canal.RabbitMQ.Models;
using RabbitMQ.Client;
using System.Reflection;

namespace Panama.Canal.RabbitMQ
{
    public class RabbitMQPolicy : IPooledObjectPolicy<IModel>
    {
        private readonly ILogger<RabbitMQPolicy> _log;
        private readonly CanalOptions _canal;
        private readonly RabbitMQOptions _options;
        private readonly string _exchange;

        private readonly IConnection _connection;

        public RabbitMQPolicy(
              ILogger<RabbitMQPolicy> log
            , IOptions<CanalOptions> canal
            , IOptions<RabbitMQOptions> options)
        {
            _log = log;
            _canal = canal.Value;
            _options = options.Value;
            _connection = GetConnection();
            _exchange = $"{_options.Exchange}.{_canal.Version}";
        }

        private IConnection GetConnection()
        {
            _log.LogInformation($"Broker Connection String: Host:{_options.Host}:{_options.Port}, VHost: {_options.VHost}");
            var factory = new ConnectionFactory() {
                Port = _options.Port,
                HostName = _options.Host.Contains(",") ? null : _options.Host,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VHost,
                DispatchConsumersAsync = true,
                ClientProvidedName = Assembly.GetEntryAssembly()?.GetName().Name!.ToLower()
            };

            if (_options.Host.Contains(","))
                return factory.CreateConnection(AmqpTcpEndpoint.ParseMultiple(_options.Host));
            else
                return factory.CreateConnection();
        }

        public IModel Create()
        {
            var model = _connection.CreateModel();
            model.ExchangeDeclare(_exchange, "topic", true);

            if (_options.PublishAcks)
                model.ConfirmSelect();

            return model;
        }

        public bool Return(IModel obj)
        {
            if (obj.IsOpen)
            {
                return true;
            }
            else
            {
                obj?.Dispose();
                return false;
            }
        }
    }
}
