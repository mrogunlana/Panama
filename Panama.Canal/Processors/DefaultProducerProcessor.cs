using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Processors
{
    public class DefaultProducerProcessor : IProcessor
    {
        private readonly IDispatcher _dispatcher;
        private readonly IServiceProvider _provider;

        public DefaultProducerProcessor(
              IDispatcher dispatcher
            , IServiceProvider provider)
        {
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

            var data = message.GetData<Message>(_provider);
            var delay = context.KvpGetSingle<string, DateTime?>("Delay") ?? data.GetDelay();

            if (delay == DateTime.MinValue)
                await _dispatcher.Publish(
                    message: message,
                    token: context.Token)
                    .ConfigureAwait(false);
            else
                await _dispatcher.Schedule(
                    message: message,
                    delay: delay,
                    token: context.Token)
                    .ConfigureAwait(false);

            return new Result().Success();
        }
    }
}