using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Channels;
using Panama.Canal.MySQL.Initializers;
using Panama.Canal.MySQL.Jobs;
using Panama.Canal.MySQL.Models;
using Panama.Extensions;
using System.Data;

namespace Panama.Canal.MySQL
{
    public static class Registrar
    {
        public static void AddPanamaCanalMySQL(this IServiceCollection services, IConfiguration config)
        {
            var settings = new MySqlSettings();
            config.GetSection("MySqlSettings").Bind(settings);

            services.AddTransient<IChannel<DatabaseFacade, IDbContextTransaction>, MySqlContextChannel>();
            services.AddTransient<IChannel<IDbConnection, IDbTransaction>, MySqlChannel>();
            services.AddTransient<IGenericChannelFactory, MySqlChannelFactory>();

            services.Remove<IStore>();
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
    }
}
