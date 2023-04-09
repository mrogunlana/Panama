using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class InboxInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IBootstrapper _bootstrapper;

        public InboxInvoker(
              IStore store
            , IBootstrapper bootstrapper)
        {
            _store = store;
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

            await _store.StoreInboxMessage(
                message: message, 
                transaction: context.Transaction)
                .ConfigureAwait(false);

            var result = new Result()
                .Success();

            return result;
        }
    }
}
