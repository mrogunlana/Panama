using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            services.AddSingleton<Canal.Store>();
            services.AddSingleton<IStore, Canal.Store>(p => p.GetRequiredService<Canal.Store>());
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

            services.Configure<StoreOptions>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Stores:Default:Options").Bind(options));
            services.Configure(_setup);
            services.PostConfigure<StoreOptions>(options => {
                options.ProcessingType = Models.ProcessingType.Poll;
            });
            services.AddSingleton<IOptions<IStoreOptions>>(p => p.GetRequiredService<IOptions<StoreOptions>>());
        }
    }
}
