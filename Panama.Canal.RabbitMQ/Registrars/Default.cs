using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Interfaces;
using Panama.Canal.RabbitMQ.Models;
using Panama.Interfaces;
using RabbitModel = RabbitMQ.Client.IModel;

namespace Panama.Canal.RabbitMQ.Registrars
{
    public class Default : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<RabbitMQOptions> _setup;

        public Type Marker => typeof(RabbitMQMarker);

        public Default(
            Panama.Models.Builder builder,
            Action<RabbitMQOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new RabbitMQMarker());

            services.AddSingleton<RabbitMQTarget>();
            services.AddSingleton<IBroker, RabbitMQBroker>();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<RabbitMQPolicy>();
            services.AddSingleton<IPooledObjectPolicy<RabbitModel>, RabbitMQPolicy>(p => p.GetRequiredService<RabbitMQPolicy>());

            services.AddSingleton<RabbitMQFactory>();
            services.AddSingleton<IBrokerFactory, RabbitMQFactory>();
            services.AddSingleton<IBrokerProcess, RabbitMQProcess>();
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

            services.Configure<RabbitMQOptions>(options =>
                _builder.Configuration.GetSection("Panama:Canal:Brokers:RabbitMQ:Options").Bind(options));

            services.Configure<RabbitMQOptions>((x) => {
                _setup(x);
            });
            services.PostConfigure<RabbitMQOptions>((x) => {
                services.AddSingleton<IBrokerOptions>(x);
            });
        }
    }
}
