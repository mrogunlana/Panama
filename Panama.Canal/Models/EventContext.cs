using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class EventContext : Context
    {
        public string Ack { get; set; } = String.Empty;
        public string Nack { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Group { get; set; } = String.Empty;
        public string Instance { get; set; } = String.Empty;
        public string SagaType { get; set; } = String.Empty;
        public string SagaId { get; set; } = String.Empty;
        public IContext? Origin { get; set; }
        public DateTime Delay { get; set; }
        public Type? Target { get; set; }
        public IDictionary<string, string?> Headers { get; set; }
        public IInvoke Invoker { get; set; }
        public IChannel? Channel { get; set; }
        public EventContext(
            IServiceProvider provider,
            IContext? origin = null)
            : base(provider)
        {
            Origin = origin;
            Headers = new Dictionary<string, string?>();
            Invoker = provider.GetRequiredService<OutboxInvoker>();
        }
    }
}
