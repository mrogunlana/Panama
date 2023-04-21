using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class PollingReceiverInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IServiceProvider _provider;

        public PollingReceiverInvoker(
              IStore store
            , IServiceProvider provider)
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

            var dispatcher = _provider.GetRequiredService<IDispatcher>();
            if (!dispatcher.Online)
                throw new InvalidOperationException("Panama Canal Dispatch service has not been started.");

            message.SetStatus(MessageStatus.Scheduled);

            var received = await _store.StoreReceivedMessage(
                message: message,
                transaction: context.Transaction)
                .ConfigureAwait(false);

            var result = new Result()
                .Success()
                .Queue(received);

            return result;
        }
    }
}
