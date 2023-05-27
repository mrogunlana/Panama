using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class BusContext : Context
    {
        public IInvoke Invoker { get; set; }
        public IChannel? Channel { get; set; }
        public IDictionary<string, string?> Headers { get; set; }
        public BusContext(
            IServiceProvider provider)
            : base(provider)
        {
            Headers = new Dictionary<string, string?>();
            Invoker = provider.GetRequiredService<PublishedInvokerFactory>().GetInvoker();
        }
    }
}
