using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using System.Transactions;

namespace Panama.Canal.Invokers
{
    public class PollingInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IBootstrap _bootstrapper;
        private readonly IServiceProvider _provider;

        public PollingInvoker(
              IStore store
            , IBootstrap bootstrapper
            , IDispatcher dispatcher
            , IServiceProvider provider)
        {
            _store = store;
            _provider = provider;
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

            message.SetStatus(MessageStatus.Scheduled);

            var published = await _store.StorePublishedMessage(
                message: message,
                transaction: Transaction.Current)
                .ConfigureAwait(false);

            var result = new Result()
                .Success()
                .Add(published);

            return result;
        }
    }
}
