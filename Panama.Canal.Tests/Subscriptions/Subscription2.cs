using Panama.Canal.Attributes;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Tests.Subscriptions
{
    [Topic("foo.created")]
    public class Subscription2 : ISubscribe
    {
        public Task Event(IContext context)
        {
            context.Add(new Kvp<string, string>("subscription.name", "Subscription2"));

            return Task.CompletedTask;
        }
    }
}
