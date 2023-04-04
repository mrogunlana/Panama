using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal
{
    public class Bus : IBus
    {
        private readonly ILogger<Bus> _log;
        
        public BusContext Context { get; }

        public Bus(
              IServiceProvider provider
            , ILogger<Bus> log)
        {
            _log = log;
            Context = new BusContext(provider);
        }

        public async Task<IResult> Post(CancellationToken? token = null)
        {
            var message = new Message()
                .AddMessageId(Guid.NewGuid().ToString())
                .AddMessageName(Context.Name)
                .AddCorrelationId(Context.CorrelationId)
                .AddMessageGroup(Context.Group)
                .AddMessageBroker(Context.Target?.FullName)
                .AddMessageInstance(Context.Instance)
                .AddMessageType(nameof(Context.Data))
                .AddCreatedTime()
                .AddDelayTime(Context.Delay)
                .AddHeaders(Context.Headers)
                .AddData(Context.Data)
                .AddAck(Context.Ack)
                .AddNack(Context.Nack)
                .ToInternal(Context.Provider);

            return await Post(message, token);
        }

        public async Task<IResult> Post(InternalMessage message, CancellationToken? token = null)
        {
            var source = CancellationTokenSource.CreateLinkedTokenSource(token ?? CancellationToken.None, Context.Token);

            var context = new Context(
                id: Context.Id,
                data: message,
                token: source.Token,
                provider: Context.Provider,
                correlationId: Context.CorrelationId,
                transaction: Context.Transaction);

            var result = await Context.Invoker.Invoke(context).ConfigureAwait(false);

            Context?.Channel?.Queue?.EnqueueResult(result);

            return result;
        }
    }
}