using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class PollingPublisherInvoker : IInvoke
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IServiceProvider _provider;
        private readonly IProcessorFactory _factory;

        public PollingPublisherInvoker(
              IStore store
            , IDispatcher dispatcher
            , IProcessorFactory factory
            , IServiceProvider provider)
        {
            _store = store;
            _factory = factory;
            _provider = provider;
            _dispatcher = dispatcher;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            if (!_dispatcher.Online)
                throw new InvalidOperationException("Panama Canal Dispatch service has not been started.");

            message.SetStatus(MessageStatus.Scheduled);

            var published = await _store.StorePublishedMessage(
                message: message,
                transaction: context.Transaction)
                .ConfigureAwait(false);

            if (context.Transaction == null)
                await _factory
                    .GetProcessor(message)
                    .Execute(new Context()
                        .Add(message)
                        .Token(context.Token));

            var result = new Result()
                .Success()
                .Queue(published);

            return result;
        }
    }
}
