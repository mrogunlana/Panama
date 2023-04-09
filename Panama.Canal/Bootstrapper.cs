using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;

namespace Panama.Canal
{
    public class Bootstrapper : IBootstrapper
    {
        private readonly IServiceProvider _provider;

        public Bootstrapper(IServiceProvider provider)
        {
            _provider = provider;
        }

        public bool Online
        {
            get 
            {
                try
                {
                    var services = _provider.GetServices<ICanalService>();
                    if (services == null)
                        return false;
                    if (services.Count() == 0)
                        return false;

                    if (services.All(x => x.Online))
                        return true;

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task Off(CancellationToken cancellationToken)
        {
            var services = _provider.GetServices<ICanalService>();
            if (services == null || services.Count() == 0)
                throw new InvalidOperationException("Panama Canal Services cannot be located.");

            foreach (var service in services)
                await service.Off(cancellationToken);
        }

        public async Task On(CancellationToken cancellationToken)
        {
            var services = _provider.GetServices<ICanalService>();
            if (services == null || services.Count() == 0)
                throw new InvalidOperationException("Panama Canal Services cannot be located.");

            foreach (var service in services)
                await service.On(cancellationToken);
        }
    }
}