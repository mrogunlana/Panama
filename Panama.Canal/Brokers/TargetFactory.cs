using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Interfaces;

namespace Panama.Canal.Brokers
{
    public class TargetFactory : ITargetFactory
    {
        private readonly IEnumerable<IBroker> _brokers;
        private readonly IServiceProvider _provider;

        public TargetFactory(
            IServiceProvider provider,
            IEnumerable<IBroker> brokers)
        {
            _brokers = brokers;
            _provider = provider;
        }
        public ITarget GetDefaultTarget()
        {
            if (_brokers == null || _brokers.Count() == 0)
                throw new InvalidOperationException($"Brokers cannot be located.");

            if (_brokers.Count() == 1)
                return (ITarget)_provider.GetRequiredService(_brokers.First().Target);

            if (_brokers.Count(b => b.Options.Default) > 1)
                throw new InvalidOperationException($"Multiple brokers cannot be registered as default.");

            var selected = _brokers.Where(b => b.Options.Default).FirstOrDefault();
            if (selected == null)
                throw new InvalidOperationException("Default broker target could not be located. Set the default option in the broker registration.");

            return (ITarget)_provider.GetRequiredService(selected.Target);
        }
    }
}
