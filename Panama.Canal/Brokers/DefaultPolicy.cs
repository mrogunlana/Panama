using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Models.Options;
using System.Reflection;

namespace Panama.Canal.Brokers
{
    public class DefaultPolicy : IPooledObjectPolicy<DefaultConnection>
    {
        private readonly ILogger<DefaultPolicy> _log;
        private readonly CanalOptions _canal;
        private readonly DefaultOptions _options;
        private readonly string _exchange;

        public DefaultPolicy(
              ILogger<DefaultPolicy> log
            , IOptions<CanalOptions> canal
            , IOptions<DefaultOptions> options)
        {
            _log = log;
            _canal = canal.Value;
            _options = options.Value;
            _exchange = $"{_options.Exchange}.{_canal.Version}";
        }

        public DefaultConnection Create()
        {
            return new DefaultConnection();
        }

        public bool Return(DefaultConnection obj)
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
