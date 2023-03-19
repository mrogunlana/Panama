using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Jobs;

namespace Panama.Canal.MySQL
{
    public static class Registrar
    {
        public static void AddPanamaCanalMySQL(this IServiceCollection services, IConfiguration config)
        {
            var settings = new MySqlSettings();
            config.GetSection("MySqlSettings").Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<IStore, Store>();
            services.AddSingleton<IInitialize, Initializer>();
            services.AddSingleton<LogTailingJob>();
            services.AddSingleton(new Job(
                type: typeof(LogTailingJob),
                expression: "0/1 * * * * ?"));

            services.Configure<MySqlOptions>(options =>
                config.GetSection(MySqlOptions.Section).Bind(options));
        }
    }
}
