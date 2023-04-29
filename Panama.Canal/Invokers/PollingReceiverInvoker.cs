using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class PollingReceiverInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IServiceProvider _provider;
        private readonly IProcessorFactory _factory;

        public PollingReceiverInvoker(
              IStore store
            , IProcessorFactory factory
            , IServiceProvider provider)
        {
            _store = store;
            _factory = factory;
            _provider = provider;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            var received = await _store.StoreReceivedMessage(
                message: message.SetStatus(MessageStatus.Scheduled))
                .ConfigureAwait(false);

            await _factory
                .GetConsumerProcessor(received)
                .Execute(new Context()
                    .Add(received)
                    .Token(context.Token))
                .ConfigureAwait(false);

            var result = new Result()
                .Success();

            return result;
        }
    }
}
