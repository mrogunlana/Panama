using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Invokers
{
    public class PollingInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IBootstrap _bootstrapper;

        public PollingInvoker(
              IStore store
            , IBootstrap bootstrapper
            , IDispatcher dispatcher)
        {
            _store = store;
            _dispatcher = dispatcher;
            _bootstrapper = bootstrapper;
        }
        public Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            if (!_bootstrapper.Online)
                throw new InvalidOperationException("Panama Canal has not been started.");


            throw new NotImplementedException();
        }
    }
}
