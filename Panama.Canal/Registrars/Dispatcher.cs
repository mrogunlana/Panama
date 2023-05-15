using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Markers;
using Panama.Canal.Models.Options;
using Panama.Interfaces;


namespace Panama.Canal.Registrars
{
    public class Dispatcher : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<DispatcherOptions> _setup;

        public Type Marker => typeof(DispatcherMarker);

        public Dispatcher(
            Panama.Models.Builder builder,
            Action<DispatcherOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new DispatcherMarker());

            services.AddSingleton<Canal.Dispatcher>();
            services.AddHostedService(p => p.GetRequiredService<Canal.Dispatcher>());
            services.AddSingleton<IDispatcher, Canal.Dispatcher>(p => p.GetRequiredService<Canal.Dispatcher>());
            services.AddSingleton<ICanalService, Canal.Dispatcher>(p => p.GetRequiredService<Canal.Dispatcher>());

            services.AddSingleton<Initializers.Sagas>();
            services.AddSingleton<Initializers.Subscriptions>();
            services.AddSingleton<Initializers.Brokers>();

            services.AddSingleton<IInitialize, Initializers.Sagas>(p => p.GetRequiredService<Initializers.Sagas>());
            services.AddSingleton<IInitialize, Initializers.Subscriptions>(p => p.GetRequiredService<Initializers.Subscriptions>());
            services.AddSingleton<IInitialize, Initializers.Brokers>(p => p.GetRequiredService<Initializers.Brokers>());
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

            services.Configure(_setup);
        }
    }
}
