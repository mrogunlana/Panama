using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Exceptions;
using Panama.Canal.Models.Options;

namespace Panama.Canal.Brokers
{
    public class BrokerFactory : IBrokerFactory
    {
        private readonly ILogger<BrokerFactory> _log;
        private readonly CanalOptions _canal;
        private readonly BrokerOptions _options;
        private readonly IServiceProvider _provider;

        public BrokerFactory(
              IServiceProvider provider
            , ILogger<BrokerFactory> log
            , IOptions<CanalOptions> canal
            , IOptions<BrokerOptions> options)
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
                var client = new BrokerClient(group, _provider);

                return client;
            }
            catch (Exception ex)
            {
                throw new BrokerException("Broker cannot be located.", ex);
            }
        }
    }
}