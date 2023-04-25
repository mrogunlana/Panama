using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Processors
{
    public class DefaultConsumerProcessor : IProcessor
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IServiceProvider _provider;

        public DefaultConsumerProcessor(
              IStore store
            , IDispatcher dispatcher
            , IServiceProvider provider)
        {
            _store = store;
            _dispatcher = dispatcher;
            _provider = provider;
        }
        public async Task<IResult> Execute(IContext context)
        {
            var _dispatcher = _provider.GetRequiredService<IDispatcher>();
            if (!_dispatcher.Online)
                throw new InvalidOperationException("Panama Canal Dispatch service has not been started.");

            var message = context.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message headers cannot be found.");

            message.SetStatus(MessageStatus.Scheduled);

            var received = await _store.StoreReceivedMessage(
                message: message)
                .ConfigureAwait(false);

            await _dispatcher.Execute(received);

            return new Result().Success();
        }
    }
}