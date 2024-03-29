﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Canal.Tests.Modules.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Modules.Subscriptions
{
    [DefaultTopic("foo.ack")]
    public class FooAcknowledged : ISubscribe
    {
        private readonly ILogger<FooCreated> _log;
        private readonly IServiceProvider _provider;

        public FooAcknowledged(
              IServiceProvider provider
            , ILogger<FooCreated> log)
        {
            _log = log;
            _provider = provider;
        }

        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(FooAcknowledged));
            
            context.Add(kvp);

            _log.LogInformation($"{typeof(FooAcknowledged)} subscriber executed.");

            var state = _provider.GetService<State>();
            if (state == null)
                return Task.CompletedTask;

            state.Data.Add(kvp);

            return Task.CompletedTask;
        }
    }
}
