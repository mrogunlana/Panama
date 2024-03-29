﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Descriptors;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Polly;

namespace Panama.Canal.Invokers
{
    public class SubscriptionInvoker : IInvoke
    {
        private readonly IInvokeFactory _invokers;
        private readonly IServiceProvider _provider;
        private readonly CanalOptions _canal;
        private readonly ILogger<SubscriptionInvoker> _log;
        private readonly IStore _store;
        private readonly SubscriberDescriptions _subscriptions;

        public SubscriptionInvoker(
              IStore store
            , IServiceProvider provider
            , IOptions<CanalOptions> canal
            , PublishedInvokerFactory invokers
            , ILogger<SubscriptionInvoker> log
            , SubscriberDescriptions subscriptions)
        {
            _log = log;
            _store = store;
            _provider = provider;
            _canal = canal.Value;
            _subscriptions = subscriptions;
            _invokers = invokers;
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
                throw new InvalidOperationException("Panama Canal services has not been started.");

            var metadata = message.GetData<Message>(_provider);
            var data = metadata.GetData<IList<IModel>>() ?? new List<IModel>();
            var group = metadata.GetGroup();

            var target = Type.GetType(metadata.GetBroker());
            if (target == null)
                throw new InvalidOperationException($"Subscription target: {metadata.GetBroker()} could not be located.");

            var local = new Polly.Context("subscription-invocation") {
                { "retry-count", 0}
            };

            try
            {
                var subscriptions = _subscriptions.GetDescriptions(target, group, metadata.GetName());
                if (subscriptions == null)
                    return new Result().Success();

                var polly = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        _canal.FailedRetryCount,
                        _ => TimeSpan.FromSeconds(_canal.FailedRetryInterval),
                        (result, timespan, retryNo, context) => {
                            _log.LogWarning($"{context.OperationKey}: Try #{retryNo} for message: {message.Id} within {timespan.TotalMilliseconds}ms.");
                            context["retry-count"] = retryNo;
                        }
                    ).ExecuteAndCaptureAsync(async (context, token) => {
                        if (token.IsCancellationRequested)
                            return new Result().Cancel();

                        foreach (var subscription in subscriptions)
                        {
                            var subscriber = (ISubscribe)_provider.GetRequiredService(subscription.Implementation);
                            if (subscriber == null)
                                throw new InvalidOperationException($"Subscriber: {subscription.Implementation.Name} could not be located.");

                            var local = new Panama.Models.Context(data.AsEnumerable(),
                                id: message.Id,
                                correlationId: message.CorrelationId,
                                provider: _provider,
                                token: token);

                            await subscriber.Event(local).ConfigureAwait(false);
                        }

                        await _store.ChangeReceivedState(metadata
                                .RemoveException()
                                .ToInternal(_provider)
                                .SetSucceedExpiration(_provider, DateTime.UtcNow.AddSeconds(_canal.SuccessfulMessageExpiredAfter)), MessageStatus.Succeeded)
                            .ConfigureAwait(false);

                        return new Result().Success();

                    }, local, context.Token)
                    .ConfigureAwait(false);

                if (metadata.HasReply())
                    await new Panama.Models.Context(_provider, context.Token).Bus()
                            .Data(data)
                            .Token(context.Token)
                            .Header(metadata.Headers.DefaultFilter())
                            .Type(data.GetType().AssemblyQualifiedName)
                            .Id(Guid.NewGuid().ToString())
                            .CorrelationId(metadata.GetCorrelationId())
                            .Topic(metadata.GetReply())
                            .Invoker(_invokers.GetInvoker())
                            .Post()
                            .ConfigureAwait(false);

                return polly.Result;
            }
            catch (Exception ex)
            {
                var reason = $"Subscription target: {metadata.GetBroker()} failed processing in subscribers.";
                
                _log.LogError(ex, reason);

                await _store.ChangeReceivedState(metadata
                        .AddException($"Exception: {ex.Message}")
                        .ToInternal(_provider)
                        .SetRetries((int)local["retry-count"])
                        .SetFailedExpiration(_provider, message.Created.AddSeconds(_canal.FailedMessageExpiredAfter)), MessageStatus.Failed)
                    .ConfigureAwait(false);

                if (metadata.HasReply())
                    await new Panama.Models.Context(_provider, context.Token).Bus()
                        .Data(data)
                        .Token(context.Token)
                        .Header(Headers.Exception, ex.Message)
                        .Header(metadata.Headers.DefaultFilter())
                        .Type(data.GetType().AssemblyQualifiedName)
                        .Id(Guid.NewGuid().ToString())
                        .CorrelationId(metadata.GetCorrelationId())
                        .Topic(metadata.GetReply())
                        .Invoker(_invokers.GetInvoker())
                        .Post()
                        .ConfigureAwait(false);

                return new Result()
                    .Fail(reason);
            }
        }
    }
}
