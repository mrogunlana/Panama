using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Models;
using Panama.Extensions;
using Panama.Models;
using RabbitMQ.Client;

namespace Panama.Canal.RabbitMQ
{
    public class RabbitMQBroker : IBroker
    {
        private readonly string _exchange;
        private readonly CanalOptions _canal;
        private readonly ILogger<RabbitMQBroker> _log;
        private readonly RabbitMQOptions _options;
        private readonly IServiceProvider _provider;

        public IPooledObjectPolicy<IModel> ConnectionPool { get; }

        IPooledObjectPolicy<Panama.Interfaces.IModel> IBroker.ConnectionPool => throw new NotImplementedException();

        public RabbitMQBroker(
              ILogger<RabbitMQBroker> log
            , IServiceProvider provider
            , IOptions<CanalOptions> canal
            , IOptions<RabbitMQOptions> options)
        {
            _log = log;
            _provider = provider;
            _canal = canal.Value;
            _options = options.Value;
            _exchange = $"{_options.Exchange}.{_canal.Version}";

            ConnectionPool = provider.GetRequiredService<RabbitMQPolicy>();
        }

        public Task<Panama.Interfaces.IResult> Publish(Panama.Interfaces.IContext context)
        {
            IModel? channel = null;
            try
            {
                channel = ConnectionPool.Create();

                var metadata = context.Data.DataGetSingle<InternalMessage>();
                var message = metadata.GetData<Message>(_provider);
                var transport = message.ToTransient(_provider);
                var properties = channel.CreateBasicProperties();

                properties.DeliveryMode = 2;
                properties.Headers = message.Headers.ToDictionary(x => x.Key, x => (object?)x.Value);

                channel.BasicPublish(_exchange, message.GetName(), properties, transport.Body);

                if (channel.NextPublishSeqNo > 0)
                    channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

                _log.LogInformation($"Panama Canal published message ({message.GetName()}). Message ID: {metadata.Id}.");

                return Task.FromResult(new Result().Success());
            }
            catch (Exception ex)
            {
                return Task.FromResult(new Result().Fail(ex.Message));
            }
            finally
            {
                if (channel != null)
                    ConnectionPool.Return(channel);
            }
        }
    }
}