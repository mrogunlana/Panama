using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Brokers
{
    public class Broker : IBroker
    {
        private readonly BrokerOptions _options;
        private readonly IBrokerObservable _observable;

        public bool Default { get; set; }

        public Type Target => typeof(DefaultTarget);

        public IBrokerOptions Options => _options;

        public Broker(
            IBrokerObservable observable,
            IOptions<BrokerOptions> options)
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
