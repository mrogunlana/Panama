using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Brokers;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Markers;
using Panama.Interfaces;

namespace Panama.Canal.Registrars
{
    public class Broker : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<BrokerOptions> _setup;

        public Type Marker => typeof(BrokerMarker);

        public Broker(
            Panama.Models.Builder builder,
            Action<BrokerOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new BrokerMarker());

            services.AddSingleton<DefaultTarget>();
            services.AddSingleton<IBroker, Brokers.Broker>();
            services.AddSingleton<IBrokerClient, BrokerClient>();
            services.AddSingleton<ITargetFactory, TargetFactory>();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<BrokerConnection>, BrokerPolicy>();

            services.AddSingleton<BrokerFactory>();
            services.AddSingleton<IBrokerFactory, BrokerFactory>();
            services.AddSingleton<IBrokerProcess, BrokerProcess>();
            services.AddSingleton<IBrokerObservable, BrokerObservable>();
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

            services.Configure<BrokerOptions>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Brokers:Default:Options").Bind(options));

            services.Configure(_setup);
        }
    }
}
