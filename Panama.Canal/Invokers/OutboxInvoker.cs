using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class OutboxInvoker : IInvoke
    {
        private readonly IServiceProvider _provider;
        private readonly IStore _store;

        public OutboxInvoker(
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

            await _store.StoreOutboxMessage(
                message: message.SetStatus(MessageStatus.Scheduled), 
                transaction: context.Transaction)
                .ConfigureAwait(false);

            var result = new Result()
                .Success();

            return result;
        }
    }
}
