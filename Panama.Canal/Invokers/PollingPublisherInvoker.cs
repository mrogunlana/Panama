using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class PollingPublisherInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IServiceProvider _provider;
        private readonly IProcessorFactory _factory;

        public PollingPublisherInvoker(
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

            var published = await _store.StorePublishedMessage(
                message: message.SetStatus(MessageStatus.Scheduled),
                transaction: context.Transaction)
                .ConfigureAwait(false);

            if (context.Transaction == null)
                await _factory
                    .GetProducerProcessor(message)
                    .Execute(new Context()
                        .Add(message)
                        .Token(context.Token))
                    .ConfigureAwait(false);

            var result = new Result()
                .Success()
                .Queue(published);

            return result;
        }
    }
}
