using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using System.Transactions;

namespace Panama.Canal.Invokers
{
    public class StreamInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IBootstrap _bootstrapper;

        public StreamInvoker(
              IStore store
            , IBootstrap bootstrapper
            , IDispatcher dispatcher)
        {
            _store = store;
            _dispatcher = dispatcher;
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

            var outbox = await _store.StoreOutboxMessage(
                message: message, 
                transaction: Transaction.Current)
                .ConfigureAwait(false);

            var result = new Result()
                .Success()
                .Add(outbox);

            return result;
        }
    }
}
