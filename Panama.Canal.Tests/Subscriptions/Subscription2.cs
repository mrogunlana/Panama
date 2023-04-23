using Microsoft.Extensions.Logging;
using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Subscriptions
{
    [DefaultTopic("foo.created")]
    public class Subscription2 : ISubscribe
    {
        private readonly ILogger<Subscription2> _log;

        public Subscription2(ILogger<Subscription2> log)
        {
            _log = log;
        }

        public Task Event(IContext context)
        {
            context.Add(new Kvp<string, string>("subscription.name", "Subscription2"));

            _log.LogInformation($"{typeof(Subscription2)} subscriber executed.");

            return Task.CompletedTask;
        }
    }
}
