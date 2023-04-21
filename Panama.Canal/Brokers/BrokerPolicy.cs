using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Models.Options;
using System.Reflection;

namespace Panama.Canal.Brokers
{
    public class BrokerPolicy : IPooledObjectPolicy<BrokerConnection>
    {
        private readonly ILogger<BrokerPolicy> _log;
        private readonly CanalOptions _canal;
        private readonly BrokerOptions _options;
        private readonly string _exchange;

        public BrokerPolicy(
              ILogger<BrokerPolicy> log
            , IOptions<CanalOptions> canal
            , IOptions<BrokerOptions> options)
        {
            _log = log;
            _canal = canal.Value;
            _options = options.Value;
            _exchange = $"{_options.Exchange}.{_canal.Version}";
        }

        public BrokerConnection Create()
        {
            return new BrokerConnection();
        }

        public bool Return(BrokerConnection obj)
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
