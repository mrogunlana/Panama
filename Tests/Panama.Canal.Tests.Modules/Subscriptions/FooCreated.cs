using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Canal.Tests.Modules.Models;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Modules.Subscriptions
{
    [DefaultTopic("foo.created")]
    public class FooCreated : ISubscribe
    {
        private readonly ILogger<FooCreated> _log;
        private readonly IServiceProvider _provider;

        public FooCreated(
              IServiceProvider provider
            , ILogger<FooCreated> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(FooCreated));

            context.Add(kvp);

            _log.LogInformation($"{typeof(FooCreated)} subscriber executed.");

            var state = _provider.GetService<State>();
            if (state == null)
                return Task.CompletedTask;

            state.Data.Add(kvp);
            state.Data.Add(new Kvp<string, DateTime>("FooCreated.DateTime", DateTime.UtcNow));

            return Task.CompletedTask;
        }
    }
}
