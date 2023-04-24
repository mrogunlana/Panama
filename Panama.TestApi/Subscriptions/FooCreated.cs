using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.TestApi.Subscriptions
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

            return Task.CompletedTask;
        }
    }
}
