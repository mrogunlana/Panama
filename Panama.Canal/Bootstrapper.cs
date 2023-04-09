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
                    var services = GetServices();
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

        public IEnumerable<ICanalService> GetServices()
        {
            var dispatcher = _provider.GetRequiredService<IDispatcher>();
            var scheduler = _provider.GetRequiredService<IScheduler>();
            
            return new List<ICanalService>() { dispatcher, scheduler };
        }

        public async Task Off(CancellationToken cancellationToken)
        {
            foreach (var service in GetServices())
                await service.Off(cancellationToken);
        }

        public async Task On(CancellationToken cancellationToken)
        {
            foreach (var service in GetServices())
                await service.On(cancellationToken);
        }
    }
}