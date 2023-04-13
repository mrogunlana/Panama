using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Polly;

namespace Panama.Canal.Invokers
{
    public class BrokerInvoker : IInvoke
    {
        private readonly IBus _bus;
        private readonly IStore _store;
        private readonly CanalOptions _canal;
        private readonly IServiceProvider _provider;
        private readonly ILogger<BrokerInvoker> _log;
        private readonly IEnumerable<IBroker> _brokers;

        public BrokerInvoker(
              IBus bus
            , IStore store
            , IServiceProvider provider
            , IEnumerable<IBroker> brokers
            , ILogger<BrokerInvoker> log
            , IOptions<CanalOptions> canal)
        {
            _bus = bus;
            _log = log;
            _store = store;
            _brokers = brokers;
            _provider = provider;
            _canal = canal.Value;
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
                throw new InvalidOperationException("Panama Canal Dispatcher has not been started.");

            var metadata = message.GetData<Message>(_provider);
            var data = message.GetData<IList<IModel>>(_provider);
            var group = metadata.GetGroup();
            var ack = metadata.GetReply();
            var target = Type.GetType(metadata.GetBroker());
            
            if (target == null)
                throw new InvalidOperationException($"Subscription target: {metadata.GetBroker()} could not be located.");

            var broker = _brokers.Where(x => x.Target == target).FirstOrDefault();
            if (broker == null)
                throw new InvalidOperationException($"Broker could not be located from target: {target.Name}");

            try
            {
                var local = new Polly.Context("broker-invocation") {
                    { "retry-count", 0}
                };
                var polly = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        _canal.FailedRetryCount,
                        _ => TimeSpan.FromSeconds(_canal.FailedRetryInterval),
                        (result, timespan, retryNo, context) => {
                            _log.LogWarning($"{context.OperationKey}: Rety #{retryNo} for message: {message.Id} within {timespan.TotalMilliseconds}ms.");
                            context["retry-count"] = retryNo;
                        }
                    ).ExecuteAndCaptureAsync(async (context, token) => {
                        if (token.IsCancellationRequested) 
                            return new Result().Cancel();

                        var result = await broker.Publish(new MessageContext(message, token: token)).ConfigureAwait(false);

                        metadata
                            .AddException($"Exception(s): {string.Join(",", result.Messages)}")
                            .ToInternal(_provider);

                        if (result.Success)
                            await _store.ChangePublishedState(metadata
                                .RemoveException()
                                .ToInternal(_provider), MessageStatus.Succeeded)
                            .ConfigureAwait(false);
                        else
                            await _store.ChangePublishedState(metadata
                                .AddException($"Exception(s): {string.Join(",", result.Messages)}")
                                .ToInternal(_provider)
                                .SetRetries((int)context["retry-count"])
                                .SetExpiration(_provider, message.Created.AddSeconds(_canal.FailedMessageExpiredAfter)), MessageStatus.Failed)
                            .ConfigureAwait(false);

                        return result;
                    }, local, context.Token);

                return polly.Result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Broker target: {metadata.GetBroker()} failed sending message.");
            }

            return new Result()
                .Success();
        }
    }
}
