using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Channels;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Models;
using Panama.Canal.Models.Markers;
using Panama.Canal.Models.Options;
using Panama.Canal.Processors;
using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Registrars
{
    public class Default : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<CanalOptions> _setup;

        public Type Marker => typeof(CanalMarker);

        public Default(
            Panama.Models.Builder builder,
            Action<CanalOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new CanalMarker());
            
            services.AddSingleton<IBootstrapper, Bootstrapper>();

            services.AddTransient<DefaultProcessor>();
            services.AddTransient<SagaProcessor>();
            services.AddTransient<IProcessor, DefaultProcessor>(p => p.GetRequiredService<DefaultProcessor>());
            services.AddTransient<IProcessor, SagaProcessor>(p => p.GetRequiredService<SagaProcessor>());
            services.AddSingleton<IProcessorFactory, ProcessorFactory>();

            services.AddTransient<IMarina, Marina>();
            services.AddTransient<IDefaultChannelFactory, DefaultChannelFactory>();

            services.AddSingleton<ISagaFactory, StatelessSagaFactory>();
            services.AddSingleton<ISagaTriggerFactory, StatelessSagaTriggerFactory>();
            services.AddSingleton<ISagaStateFactory, StatelessSagaStateFactory>();

            services.AddSingleton<ConsumerSubscriptions>();
            services.AddSingleton<PublishedInvokerFactory>();
            services.AddSingleton<ReceivedInvokerFactory>();
        }

        public void AddAssemblies(IServiceCollection services)
        {
            if (_builder.Assemblies == null)
                return;

            services.AddAssemblyTypes<IInvoke>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IChannel>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<ISagaState>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaTrigger>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaEvent>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<ISubscribe>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypesWithInterface<ISubscribe>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypesByInterface<IInvokeFactory>(_builder.Assemblies.Distinct(), true);
        }

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;

            var options = new CanalOptions();

            options.SetBuilder(new Panama.Models.Builder(_builder.Configuration, _builder.Assemblies));

            _setup(options);

            foreach (var registrar in options.Builder.Registrars)
            {
                if (services.Exist(registrar.Marker))
                    continue;

                registrar.AddServices(services);
                registrar.AddAssemblies(services);
                registrar.AddConfigurations(services);
            }

            services.Configure(_setup);

            options.Builder?.Assemblies?.Clear();
        }
    }
}
