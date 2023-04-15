using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class BusContext : Context
    {
        public string Reply { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string? Group { get; set; }
        public string Instance { get; set; } = String.Empty;
        public string SagaType { get; set; } = String.Empty;
        public string SagaId { get; set; } = String.Empty;
        public IContext? Origin { get; set; }
        public DateTime Delay { get; set; }
        public Type? Target { get; set; }
        public IDictionary<string, string?> Headers { get; set; }
        public IInvoke Invoker { get; set; }
        public IChannel? Channel { get; set; }
        public BusContext(
            IServiceProvider provider,
            IContext? origin = null)
            : base(provider)
        {
            Origin = origin;
            Headers = new Dictionary<string, string?>();
            Invoker = provider.GetRequiredService<PublishedInvokerFactory>().GetInvoker();
        }
    }
}
