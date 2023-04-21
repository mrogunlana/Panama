using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Markers;
using Panama.Canal.MySQL.Channels;
using Panama.Canal.MySQL.Jobs;
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
            services.AddSingleton(new MySqlMarker());

            services.AddTransient<IChannel<DatabaseFacade, IDbContextTransaction>, MySqlContextChannel>();
            services.AddTransient<IChannel<IDbConnection, IDbTransaction>, MySqlChannel>();
            services.AddTransient<IGenericChannelFactory, MySqlChannelFactory>();

            services.Remove<IStore>();
            services.Remove<Canal.Store>();
            services.AddSingleton<IStore, Store>();
            services.AddSingleton<IInitialize, Initializers.Default>();
            services.AddSingleton<LogTailingJob>();
            services.AddSingleton(new Job(
                type: typeof(LogTailingJob),
                expression: "0/1 * * * * ?"));
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

            services.Configure<MySqlSettings>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Stores:Mysql:Settings").Bind(options));
        }
    }
}
