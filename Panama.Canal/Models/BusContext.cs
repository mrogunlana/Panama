using Panama.Canal.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class BusContext : Context
    {
        public IBus Instance { get; }
        public string Ack { get; set; } = String.Empty;
        public string Nack { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Group { get; set; } = String.Empty;
        public string BrokerInstance { get; set; } = String.Empty;
        public TimeSpan Delay { get; set; }
        public IBroker? Broker { get; set; }
        public AsyncLocal<ITransaction> Transaction { get; set; }
        public IDictionary<string, string?> Headers { get; set; }
        public BusContext(IBus instance)
        {
            Instance = instance;
            Transaction = new AsyncLocal<ITransaction>();
            Headers = new Dictionary<string, string?>();
        }
        public BusContext(
            IBus instance,
            IServiceProvider provider)
            : base(provider)
        {
            Instance = instance;
            Transaction = new AsyncLocal<ITransaction>();
            Headers = new Dictionary<string, string?>();
        }
    }
}
