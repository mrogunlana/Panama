using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Brokers
{
    public class DefaultBrokerFactory : IBrokerFactory
    {
        private readonly ILogger<DefaultBrokerFactory> _log;
        private readonly IOptions<CanalOptions> _canal;
        private readonly IPooledObjectPolicy<IModel> _models;
        private readonly IOptions<DefaultOptions> _options;
        private readonly IServiceProvider _provider;

        public DefaultBrokerFactory(
              IServiceProvider provider
            , ILogger<DefaultBrokerFactory> log
            , IOptions<CanalOptions> canal
            , IOptions<DefaultOptions> options
            , IPooledObjectPolicy<IModel> models)
        {
            _log = log;
            _canal = canal;
            _models = models;
            _options = options;
            _provider = provider;
        }
        public IBrokerClient Create(string group)
        {
            try
            {
                var client = new DefaultClient(group, _provider);

                return client;
            }
            catch (Exception ex)
            {
                throw new BrokerException("Broker cannot be located.", ex);
            }
        }
    }
}