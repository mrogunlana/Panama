using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.RabbitMQ.Models;
using RabbitMQ.Client;

namespace Panama.Canal.RabbitMQ
{
    public class RabbitMQFactory : IBrokerFactory
    {
        private readonly ILogger<RabbitMQFactory> _log;
        private readonly IOptions<CanalOptions> _canal;
        private readonly IOptions<RabbitMQOptions> _options;
        private readonly IPooledObjectPolicy<IModel> _models;

        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }

        public RabbitMQFactory(
              ILogger<RabbitMQFactory> log
            , IOptions<CanalOptions> canal
            , IOptions<RabbitMQOptions> options
            , IPooledObjectPolicy<IModel> models)
        {
            _log = log;
            _canal = canal;
            _models = models;
            _options = options;
        }

        public IBrokerClient Create(string group)
        {
            try
            {
                var client = new RabbitMQClient(group, _canal, _options, _models);

                client.Connect();
                
                return client;
            }
            catch (Exception ex)
            {
                throw new BrokerException("Broker cannot be located.", ex);
            }
        }
    }
}