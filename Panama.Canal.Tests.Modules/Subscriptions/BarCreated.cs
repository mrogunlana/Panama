using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Panama.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Tests.Modules.Models;

namespace Panama.Canal.Tests.Modules.Subscriptions
{
    [DefaultTopic("bar.created", "test.queue")]
    public class BarCreated : ISubscribe
    {
        private readonly ILogger<BarCreated> _log;
        private readonly IServiceProvider _provider;

        public BarCreated(
              IServiceProvider provider
            , ILogger<BarCreated> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(BarCreated));

            context.Add(kvp);

            _log.LogInformation($"{typeof(BarCreated)} subscriber executed.");

            var state = _provider.GetService<State>();
            if (state == null)
                return Task.CompletedTask;

            state.Data.Add(kvp);

            return Task.CompletedTask;
        }
    }
}
