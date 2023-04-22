using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Markers;
using Panama.Canal.MySQL.Channels;
using Panama.Canal.MySQL.Models;
using Panama.Extensions;
using Panama.Interfaces;
using System.Data;

namespace Panama.Canal.MySQL.Registrars
{
    public class Default : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<MySqlOptions> _setup;

        public Type Marker => typeof(StoreMarker);

        public Default(
            Panama.Models.Builder builder,
            Action<MySqlOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new StoreMarker());

            services.AddTransient<IChannel<DatabaseFacade, IDbContextTransaction>, MySqlContextChannel>();
            services.AddTransient<IChannel<IDbConnection, IDbTransaction>, MySqlChannel>();
            services.AddTransient<IGenericChannelFactory, MySqlChannelFactory>();

            services.AddSingleton<Tailer>();
            services.AddHostedService(p => p.GetRequiredService<Tailer>());
            services.AddSingleton<ITailer, Tailer>(p => p.GetRequiredService<Tailer>());
            services.AddSingleton<ICanalService, Tailer>(p => p.GetRequiredService<Tailer>());

            services.AddSingleton<Store>();
            services.AddSingleton<IStore, Store>(p => p.GetRequiredService<Store>());
            services.AddSingleton<IInitialize, Initializers.Default>();
        }

        public void AddAssemblies(IServiceCollection services)
        {
            if (_builder.Assemblies == null)
                return;

            services.AddAssemblyTypes<IInvoke>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IChannel>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IInitialize>(_builder.Assemblies.Distinct(), true);
        }

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;

            services.Configure<MySqlOptions>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Stores:Mysql:Options").Bind(options));

            services.PostConfigure<MySqlOptions>(options => {
                services.AddSingleton<IStoreOptions>(options);
            });
            services.AddSingleton<IOptions<IStoreOptions>>(p => p.GetRequiredService<IOptions<MySqlOptions>>());

            services.Configure<MySqlSettings>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Stores:Mysql:Settings").Bind(options));
        }
    }
}
