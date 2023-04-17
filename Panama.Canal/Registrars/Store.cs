using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Markers;
using Panama.Canal.Models.Options;
using Panama.Interfaces;

namespace Panama.Canal.Registrars
{
    public class Store : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<StoreOptions> _setup;

        public Type Marker => typeof(StoreMarker);

        public Store(
            Panama.Models.Builder builder,
            Action<StoreOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new StoreMarker());

            services.AddSingleton<IStore, Canal.Store>();
            services.AddSingleton<Canal.Store>();
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

            var storeOptions = new StoreOptions();
            _builder.Configuration.GetSection("Panama:Canal:Store").Bind(storeOptions);

            services.Configure(_setup);
        }
    }
}
