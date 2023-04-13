using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Brokers
{
    public class DefaultBroker : IBroker
    {
        private readonly DefaultOptions _options;
        private readonly IPooledObjectPolicy<DefaultConnection> _connections;

        public bool Default { get; set; }

        public Type Target => typeof(DefaultTarget);

        public IBrokerOptions Options => _options;

        public DefaultBroker(
            IOptions<DefaultOptions> options,
            IPooledObjectPolicy<DefaultConnection> connections)
        {
            _options = options.Value;
            _connections = connections;
        }

        public Task<IResult> Publish(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
