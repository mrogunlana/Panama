using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Samples.TestApi.Subscriptions
{
    [DefaultTopic("foo.created")]
    public class CreatedFoo : ISubscribe
    {
        private readonly ILogger<CreatedFoo> _log;
        private readonly IServiceProvider _provider;

        public CreatedFoo(
              IServiceProvider provider
            , ILogger<CreatedFoo> log)
        {
            _log = log;
            _provider = provider;
        }
        public Task Event(IContext context)
        {
            var kvp = new Kvp<string, string>("subscription.name", nameof(CreatedFoo));

            context.Add(kvp);

            _log.LogInformation($"{typeof(CreatedFoo)} subscriber executed.");

            return Task.CompletedTask;
        }
    }
}
