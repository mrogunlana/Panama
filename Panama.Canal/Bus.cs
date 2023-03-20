using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal
{
    public class Bus : IBus
    {
        private readonly ILogger<Bus> _log;
        private readonly IDispatcher _dispatcher;
        private readonly IBootstrap _bootstrapper;
        public IServiceProvider ServiceProvider { get; }
        public BusContext Context { get; }

        public Bus(
              IServiceProvider provider
            , IBootstrap bootstrapper
            , IDispatcher dispatcher
            , ILogger<Bus> log)
        {
            _log = log;
            _dispatcher = dispatcher;
            _bootstrapper = bootstrapper;

            ServiceProvider = provider;
            Context = new BusContext(this, provider);
        }

        public async Task Publish(CancellationToken? token = null)
        {
            if (!_bootstrapper.Online)
                throw new InvalidOperationException("Panama Canal has not been started.");

            var message = new Message()
                .AddMessageId(Context.Id)
                .AddMessageName(Context.Name)
                .AddCorrelationId(Context.CorrelationId)
                .AddMessageGroup(Context.Group)
                .AddMessageBroker(nameof(Context.Broker))
                .AddMessageType(nameof(Context.Data))
                .AddCreatedTime()
                .AddDelayTime(Context.Delay)
                .AddHeaders(Context.Headers)
                .AddData(Context.Data)
                .AddAck(Context.Ack)
                .AddNack(Context.Nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message, token: token).ConfigureAwait(false);
        }
    }
}