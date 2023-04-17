using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Panama.Canal.Brokers;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Channels;
using Panama.Canal.Interfaces;
using Panama.Canal.Invokers;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Models.Markers;
using Panama.Canal.Models.Options;
using Panama.Canal.Processors;
using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Quartz;

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

            services.AddHostedService<Dispatcher>();
            services.AddHostedService<Scheduler>();
            services.AddSingleton<IBootstrapper, Bootstrapper>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<Interfaces.IScheduler, Scheduler>();

            services.AddTransient<IProcessor, DefaultProcessor>();
            services.AddTransient<IProcessor, SagaProcessor>();
            services.AddSingleton<IProcessorFactory, ProcessorFactory>();

            services.AddTransient<IBus, Bus>();
            services.AddTransient<IDefaultChannelFactory, DefaultChannelFactory>();
            services.AddSingleton<DefaultTarget>();
            services.AddSingleton<ITargetFactory, TargetFactory>();

            services.AddSingleton<ISagaFactory, StatelessSagaFactory>();
            services.AddSingleton<ISagaTriggerFactory, StatelessSagaTriggerFactory>();
            services.AddSingleton<ISagaStateFactory, StatelessSagaStateFactory>();
            services.AddSingleton<ConsumerSubscriptions>();
            services.AddSingleton<PublishedInvokerFactory>();
            services.AddSingleton<ReceivedInvokerFactory>();

            services.AddSingleton<ReceivedRetry>();
            services.AddSingleton<DeleteExpired>();
            services.AddSingleton<PublishedRetry>();
            services.AddSingleton<DelayedPublished>();

            services.AddSingleton(new Job(
                type: typeof(DelayedPublished),
                expression: "0 */1 * ? * *"));
            services.AddSingleton(new Job(
                type: typeof(PublishedRetry),
                expression: "0 */1 * ? * *"));
            services.AddSingleton(new Job(
                type: typeof(ReceivedRetry),
                expression: "0 */1 * ? * *"));
            services.AddSingleton(new Job(
                type: typeof(DeleteExpired),
                expression: "0 */5 * ? * *"));

            services.AddQuartz(q => {
                q.SchedulerName = "panama-canal-services";
                q.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(
                q => q.WaitForJobsToComplete = true);
        }

        public void AddAssemblies(IServiceCollection services)
        {
            if (_builder.Assemblies == null)
                return;

            services.AddAssemblyTypes<IInvoke>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IChannel>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IInitialize>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaState>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaTrigger>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaEvent>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypesByInterface<ISubscribe>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypesByInterface<IInitialize>(_builder.Assemblies.Distinct(), true);
            services.AddAssemblyTypesByInterface<IInvokeFactory>(_builder.Assemblies.Distinct(), true);
        }

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;
            
            services.Configure(_setup);
        }
    }
}
