using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Extensions
{
    public static class BrokerExtensions
    {
        public static IBroker GetTargetBroker(this IEnumerable<IBroker> broker, Type? target = null)
        {
            if (broker == null)
                throw new ArgumentNullException(nameof(broker));

            var selected = broker.Where(b => b.Target == target).FirstOrDefault();
            if (selected != null)
                return selected;

            selected = broker.Where(b => b.Target == typeof(DefaultTarget)).FirstOrDefault();
            if (selected == null)
                throw new InvalidOperationException("Default broker subscription targets could not be located.");

            return selected;
        }
    }
}
