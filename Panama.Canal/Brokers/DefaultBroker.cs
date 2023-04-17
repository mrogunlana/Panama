using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Brokers
{
    public class DefaultBroker : IBroker
    {
        private readonly DefaultOptions _options;
        private readonly IDefaultObservable _observable;

        public bool Default { get; set; }

        public Type Target => typeof(DefaultTarget);

        public IBrokerOptions Options => _options;

        public DefaultBroker(
            IDefaultObservable observable,
            IOptions<DefaultOptions> options)
        {
            _observable = observable;
            _options = options.Value;
        }

        public Task<IResult> Publish(IContext context)
        {
            var message = context.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new ArgumentNullException(nameof(InternalMessage));

            _observable.Publish(message);

            return Task.FromResult(new Result().Success());
        }
    }
}
