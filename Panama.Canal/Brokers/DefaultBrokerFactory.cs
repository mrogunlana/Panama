using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Models.Options;

namespace Panama.Canal.Brokers
{
    public class DefaultBrokerFactory : IBrokerFactory
    {
        private readonly ILogger<DefaultBrokerFactory> _log;
        private readonly CanalOptions _canal;
        private readonly DefaultOptions _options;
        private readonly IServiceProvider _provider;

        public DefaultBrokerFactory(
              IServiceProvider provider
            , ILogger<DefaultBrokerFactory> log
            , IOptions<CanalOptions> canal
            , IOptions<DefaultOptions> options)
        {
            _log = log;
            _provider = provider;
            _canal = canal.Value;
            _options = options.Value;
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