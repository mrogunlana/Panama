using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal
{
    public class Bus : IBus
    {
        private readonly ILogger<Bus> _log;
        private readonly IOptions<CanalOptions> _options;
        private readonly ITargetFactory _targets;

        public BusContext Context { get; }

        public Bus(
              ILogger<Bus> log,
              ITargetFactory targets,
              IServiceProvider provider,
              IOptions<CanalOptions> options)
        {
            _log = log;
            _options = options;
            _targets = targets;

            Context = new BusContext(provider);
        }

        public async Task<IResult> Post(CancellationToken? token = null)
        {
            var message = new Message()
                .AddMessageId(Guid.NewGuid().ToString())
                .AddMessageName(Context.Name)
                .AddCorrelationId(Context.CorrelationId)
                .AddMessageGroup(Context.Group ?? _options.Value.DefaultGroup)
                .AddMessageBroker(Context.Target?.FullName ?? _targets.GetDefaultTarget().GetType().AssemblyQualifiedName)
                .AddMessageInstance(Context.Instance)
                .AddMessageType(nameof(Context.Data))
                .AddCreatedTime()
                .AddDelayTime(Context.Delay)
                .AddHeaders(Context.Headers)
                .AddData(Context.Data)
                .AddReply(Context.Reply)
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