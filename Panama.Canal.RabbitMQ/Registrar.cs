using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using System.Reflection;

namespace Panama.Canal.RabbitMQ
{
    public static class Registrar
    {
        private static void AddPanamaCanalRabbitMQBase(this IServiceCollection services, IConfiguration config)
        {
            //services.AddSingleton<IInitialize, Intializers.Default>();
            //services.AddSingleton<LogTailingJob>();
            //services.AddSingleton(new Job(
            //    type: typeof(LogTailingJob),
            //    expression: "0/1 * * * * ?"));

            //services.Configure<MySqlOptions>(options =>
            //    config.GetSection(MySqlOptions.Section).Bind(options));
        }

        public static void AddPanamaCanalRabbitMQ(this IServiceCollection services, IConfiguration config)
        {
            AddPanamaCanalRabbitMQBase(services, config);
        }

        public static void AddPanamaCanalRabbitMQ(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();

            AddPanamaCanalRabbitMQBase(services, config);

            //foreach (var assembly in assembliesToScan)
            //    services.AddAssemblyType(typeof(ISubscribe), assembly, false);
        }
    }
}
