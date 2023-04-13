using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.RabbitMQ.Models;
using RabbitMQ.Client;
using System.Reflection;

namespace Panama.Canal.RabbitMQ
{
    public static class Registrar
    {
        private static void AddPanamaCanalRabbitMQBase(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<IModel>, RabbitMQPolicy>();
            services.AddSingleton<IBroker, RabbitMQBroker>();
            services.AddSingleton<IBrokerFactory, RabbitMQFactory>();
            services.AddSingleton<IBrokerProcess, Jobs.Default>();
            
            services.AddSingleton(new Job(
                type: typeof(Jobs.Default),
                expression: "0/1 * * * * ?"));

            services.Configure<RabbitMQOptions>(options =>
                config.GetSection(RabbitMQOptions.Section).Bind(options));
        }

        public static void AddPanamaCanalRabbitMQ(this IServiceCollection services, IConfiguration config, Action<RabbitMQOptions>? setup = null)
        {
            AddPanamaCanalRabbitMQBase(services, config);

            var options = new RabbitMQOptions();
            config.GetSection(RabbitMQOptions.Section).Bind(options);

            if (setup != null)
                setup(options);

            services.AddSingleton(options);
        }

        public static void AddPanamaCanalRabbitMQ(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly> assemblies, Action<RabbitMQOptions>? setup = null)
        {
            AddPanamaCanalRabbitMQBase(services, config);

            var options = new RabbitMQOptions();
            config.GetSection(RabbitMQOptions.Section).Bind(options);

            if (setup != null)
                setup(options);

            services.AddSingleton(options);
            services.AddSingleton<IBrokerOptions>(options);
        }
    }
}
