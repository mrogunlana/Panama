using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class BrokerInvoker : IInvoke
    {
        private readonly IBus _bus;
        private readonly ILogger<BrokerInvoker> _log;
        private readonly IEnumerable<IBroker> _brokers;
        private readonly IBootstrap _bootstrapper;
        private readonly IServiceProvider _provider;

        public BrokerInvoker(
              IBus bus
            , IBootstrap bootstrapper
            , IServiceProvider provider
            , IEnumerable<IBroker> brokers
            , ILogger<BrokerInvoker> log)
        {
            _bus = bus;
            _log = log;
            _brokers = brokers;
            _provider = provider;
            _bootstrapper = bootstrapper;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            if (!_bootstrapper.Online)
                throw new InvalidOperationException("Panama Canal has not been started.");

            var metadata = message.GetData<Message>(_provider);
            var data = message.GetData<IList<IModel>>(_provider);
            var group = metadata.GetGroup();
            var instance = metadata.GetInstance();
            var ack = metadata.GetAck();
            var nack = metadata.GetNack();
            var target = Type.GetType(metadata.GetBroker());
            
            if (target == null)
                throw new InvalidOperationException($"Subscription target: {metadata.GetBroker()} could not be located.");

            try
            {
                
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Broker target: {metadata.GetBroker()} failed sending message.");
            }

            var result = new Result()
                .Success();

            return result;
        }
    }
}
