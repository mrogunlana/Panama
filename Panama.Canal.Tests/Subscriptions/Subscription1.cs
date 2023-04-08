using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Interfaces;
using Panama.Models;
using Panama.Extensions;

namespace Panama.Canal.Tests.Subscriptions
{
    [Topic("bar.created", "test.queue")]
    public class Subscription1 : ISubscribe
    {
        public Task Event(IContext context)
        {
            context.Add(new Kvp<string, string>("subscription.name", "Subscription1"));

            return Task.CompletedTask;
        }
    }
}
