using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class BusContext : Context
    {
        public IBus Current { get; }
        public string Ack { get; set; } = String.Empty;
        public string Nack { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Group { get; set; } = String.Empty;
        public string Instance { get; set; } = String.Empty;
        public DateTime Delay { get; set; }
        public IBroker? Broker { get; set; }
        public AsyncLocal<ITransaction> Transaction { get; set; }
        public IDictionary<string, string?> Headers { get; set; }
        public IInvoke Invoker { get; set; }
        public BusContext(
            IBus instance,
            IServiceProvider provider)
            : base(provider)
        {
            Current = instance;
            Transaction = new AsyncLocal<ITransaction>();
            Headers = new Dictionary<string, string?>();
            Invoker = provider.GetRequiredService<StreamInvoker>();
        }
    }
}
