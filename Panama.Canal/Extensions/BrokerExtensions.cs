using Panama.Canal.Interfaces;

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

            selected = broker.Where(b => b.Default).FirstOrDefault();
            if (selected == null)
                throw new InvalidOperationException("Default broker target could not be located.");

            return selected;
        }
    }
}
