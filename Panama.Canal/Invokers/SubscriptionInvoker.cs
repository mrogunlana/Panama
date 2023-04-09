using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Invokers
{
    public class SubscriptionInvoker : IInvoke
    {
        private readonly ILogger<SubscriptionInvoker> _log;
        private readonly IServiceProvider _provider;
        private readonly ConsumerSubscriptions _subscriptions;

        public SubscriptionInvoker(
              IServiceProvider provider
            , ConsumerSubscriptions subscriptions
            , ILogger<SubscriptionInvoker> log)
        {
            _log = log;
            _provider = provider;
            _subscriptions = subscriptions;
        }
        public async Task<IResult> Invoke(IContext? context = null)
        {
            if (context == null)
                throw new ArgumentNullException("Context cannot be located.");

            var message = context.Data.DataGetSingle<InternalMessage>();
            if (message == null)
                throw new InvalidOperationException("Message cannot be located.");

            var bootstrapper = _provider.GetRequiredService<IBootstrapper>();
            if (!bootstrapper.Online)
                throw new InvalidOperationException("Panama Canal has not been started.");

            var metadata = message.GetData<Message>(_provider);
            var data = message.GetData<IList<IModel>>(_provider);
            var group = metadata.GetGroup();

            var target = Type.GetType(metadata.GetBroker());
            if (target == null)
                throw new InvalidOperationException($"Subscription target: {metadata.GetBroker()} could not be located.");

            try
            {
                var subscriptions = _subscriptions.GetSubscriptions(target, group, metadata.GetName());
                if (subscriptions == null)
                    return new Result().Success();

                foreach (var subscription in subscriptions)
                {
                    var subscriber = (ISubscribe)_provider.GetRequiredService(subscription.Subscriber);
                    if (subscriber == null)
                        throw new InvalidOperationException($"Subscriber: {subscription.Subscriber.Name} could not be located.");

                    var local = new Context(data.AsEnumerable(),
                        id: message.Id,
                        correlationId: message.CorrelationId,
                        provider: _provider,
                        token: context.Token);

                    await subscriber.Event(local).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var reason = $"Subscription target: {metadata.GetBroker()} failed processing in subscribers.";
                
                _log.LogError(ex, reason);

                if (!string.IsNullOrEmpty(metadata.GetReply()))
                    await _provider.GetRequiredService<IBus>()
                        .Instance(metadata.GetInstance())
                        .Token(context.Token)
                        .Header(Headers.Exception, ex.Message)
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(metadata.GetCorrelationId())
                        .Topic(metadata.GetReply())
                        .Group(group)
                        .Data(data)
                        .Stream()
                        .Target(target)
                        .SagaId(metadata.GetSagaId())
                        .SagaType(metadata.GetSagaType())
                        .Post()
                        .ConfigureAwait(false);

                return new Result()
                    .Fail(reason);
            }

            if (!string.IsNullOrEmpty(metadata.GetReply()))
                await _provider.GetRequiredService<IBus>()
                        .Instance(metadata.GetInstance())
                        .Token(context.Token)
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(metadata.GetCorrelationId())
                        .Topic(metadata.GetReply())
                        .Group(group)
                        .Data(data)
                        .Stream()
                        .Target(target)
                        .SagaId(metadata.GetSagaId())
                        .SagaType(metadata.GetSagaType())
                        .Post()
                        .ConfigureAwait(false);

            return new Result()
                .Success();
        }
    }
}
