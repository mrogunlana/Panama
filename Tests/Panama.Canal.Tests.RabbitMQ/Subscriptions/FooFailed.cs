using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Attributes;
using Panama.Canal.Tests.Modules.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.RabbitMQ.Subscriptions
{
    [RabbitTopic("foo.failed")]
    public class FooFailed : ISubscribe
    {
        private readonly ILogger<FooCreated> _log;
        private readonly IServiceProvider _provider;

        public FooFailed(
              IServiceProvider provider
            , ILogger<FooCreated> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(FooFailed));

            context.Add(kvp);

            _log.LogInformation($"{typeof(FooFailed)} subscriber executed.");

            var state = _provider.GetService<State>();
            if (state == null)
                return Task.CompletedTask;

            state.Data.Add(kvp);

            return Task.CompletedTask;
        }
    }
}
