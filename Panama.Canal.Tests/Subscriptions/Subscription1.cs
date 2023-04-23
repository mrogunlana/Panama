using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Panama.Extensions;
using Microsoft.Extensions.Logging;

namespace Panama.Canal.Tests.Subscriptions
{
    [DefaultTopic("bar.created", "test.queue")]
    public class Subscription1 : ISubscribe
    {
        private readonly ILogger<Subscription1> _log;

        public Subscription1(ILogger<Subscription1> log)
        {
            _log = log;
        }
        public Task Event(IContext context)
        {
            context.Add(new Kvp<string, string>("subscription.name", "Subscription1"));

            _log.LogInformation($"{typeof(Subscription1)} subscriber executed.");

            return Task.CompletedTask;
        }
    }
}
