using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Brokers;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models.Markers;
using Panama.Interfaces;

namespace Panama.Canal.Registrars
{
    public class Broker : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<DefaultOptions> _setup;

        public Type Marker => typeof(BrokerMarker);

        public Broker(
            Panama.Models.Builder builder,
            Action<DefaultOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new BrokerMarker());

            services.AddSingleton<IBroker, DefaultBroker>();
            services.AddSingleton<IBrokerClient, DefaultClient>();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<DefaultConnection>, DefaultPolicy>();

            services.AddSingleton<DefaultBrokerFactory>();
            services.AddSingleton<IBrokerFactory, DefaultBrokerFactory>();
        }

        public void AddAssemblies(IServiceCollection services)
        {
            if (_builder.Assemblies == null)
                return;
        }

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;
            
            var brokerOptions = new DefaultOptions();
            _builder.Configuration.GetSection("Panama:Canal:Broker").Bind(brokerOptions);

            services.Configure(_setup);
        }
    }
}
