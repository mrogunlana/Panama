﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal
{
    public class Bus : IBus
    {
        private readonly ILogger<Bus> _log;
        private readonly IOptions<CanalOptions> _options;
        private readonly IServiceProvider _provider;
        private readonly ITargetFactory _targets;

        public BusContext Context { get; }

        public Bus(IServiceProvider provider)
        {
            _provider = provider;
            _log = provider.GetRequiredService<ILogger<Bus>>();
            _targets = provider.GetRequiredService<ITargetFactory>();
            _options = provider.GetRequiredService<IOptions<CanalOptions>>();

            Context = new BusContext(provider);
        }

        public async Task<IResult> Post(CancellationToken? token = null)
        {
            var message = new Message()
                .AddHeaders(Context.Headers)
                .AddMessageId(Context.Id)
                .AddCorrelationId(Context.CorrelationId)
                .AddReply(_options.Value.GetName(Context.Headers.GetReply()))
                .AddGroup(Context.Headers.GetGroup() ?? _options.Value.DefaultGroup)
                .AddBroker(Context.Headers.GetBroker() ?? _targets.GetDefaultTarget().GetType().AssemblyQualifiedName)
                .AddType(Context.Data.GetType().AssemblyQualifiedName)
                .AddCreatedTime()
                .AddData(Context.Data)
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