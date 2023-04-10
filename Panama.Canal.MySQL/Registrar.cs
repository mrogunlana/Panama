using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Channels;
using Panama.Canal.MySQL.Initializers;
using Panama.Canal.MySQL.Jobs;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using System.Data;
using System.Reflection;

namespace Panama.Canal.MySQL
{
    public static class Registrar
    {
        public static void AddPanamaCanalMySQLBase(this IServiceCollection services, IConfiguration config)
        {
            var settings = new MySqlSettings();
            config.GetSection("MySqlSettings").Bind(settings);

            services.AddTransient<IChannel<DatabaseFacade, IDbContextTransaction>, MySqlContextChannel>();
            services.AddTransient<IChannel<IDbConnection, IDbTransaction>, MySqlChannel>();
            services.AddTransient<IGenericChannelFactory, MySqlChannelFactory>();

            services.Remove<IStore>();
            services.Remove<Canal.Store>();
            services.AddSingleton(settings);
            services.AddSingleton<IStore, Store>();
            services.AddSingleton<IInitialize, Default>();
            services.AddSingleton<LogTailingJob>();
            services.AddSingleton(new Job(
                type: typeof(LogTailingJob),
                expression: "0/1 * * * * ?"));

            services.Configure<MySqlOptions>(options =>
                config.GetSection(MySqlOptions.Section).Bind(options));
        }

        public static void AddPanamaCanalMySQL(this IServiceCollection services, IConfiguration config)
        {
            AddPanamaCanalMySQLBase(services, config);

            services.AddAssemblyType(typeof(IInvoke), Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyType(typeof(IChannel), Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyType(typeof(IInitialize), Assembly.GetEntryAssembly()!, true);
        }

        public static void AddPanamaCanalMySQL(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly> assemblies)
        {
            AddPanamaCanalMySQLBase(services, config);

            services.AddAssemblyTypes<IInvoke>(assemblies.Distinct(), false);
            services.AddAssemblyTypes<IChannel>(assemblies.Distinct(), false);
            services.AddAssemblyTypes<IInitialize>(assemblies.Distinct(), true);
        }
    }
}
