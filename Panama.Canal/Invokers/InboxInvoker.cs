using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Quartz.Impl.AdoJobStore.Common;

namespace Panama.Canal.Invokers
{
    public class InboxInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IServiceProvider _provider;

        public InboxInvoker(
              IStore store,
              IServiceProvider provider)
        {
            _store = store;
            _provider = provider;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            var bootstrapper = _provider.GetRequiredService<IBootstrapper>();
            if (!bootstrapper.Online)
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
