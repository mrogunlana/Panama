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
using Panama.Canal.Processors;
using Panama.Canal.Sagas.Stateless;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Quartz;
using System.Reflection;

namespace Panama.Canal
{
    public static class Registrar
    {
        private static void AddPanamaCanalBase(this IServiceCollection services, IConfiguration config)
        {
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
            services.AddSingleton<IBroker, DefaultBroker>();
            services.AddSingleton<IBrokerClient, DefaultClient>();
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<IPooledObjectPolicy<DefaultConnection>, DefaultPolicy>();

            services.AddSingleton<IStore, Store>();
            services.AddSingleton<Store>();
            services.AddSingleton<ISagaFactory, StatelessSagaFactory>();
            services.AddSingleton<ISagaTriggerFactory, StatelessSagaTriggerFactory>();
            services.AddSingleton<ISagaStateFactory, StatelessSagaStateFactory>();
            services.AddSingleton<ConsumerSubscriptions>();
            services.AddSingleton<PublishedInvokerFactory>();
            services.AddSingleton<ReceivedInvokerFactory>();
            services.AddSingleton<DefaultBrokerFactory>();
            services.AddSingleton<IBrokerFactory, DefaultBrokerFactory>();

            services.AddSingleton<MemorySettings>();
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

        public static void AddPanamaCanal(this IServiceCollection services, IConfiguration config, Action<CanalOptions>? setup = null)
        {
            AddPanamaCanalBase(services, config);

            var settings = new MemorySettings();
            config.GetSection("MemorySettings").Bind(settings);

            var brokerOptions = new DefaultOptions();
            config.GetSection("DefaultBrokerSettings").Bind(brokerOptions);


            var options = new CanalOptions();
            config.GetSection(CanalOptions.Section).Bind(options);

            if (setup  != null) 
                setup(options);

            services.AddSingleton(options);
            services.AddSingleton(settings);
            services.AddSingleton(brokerOptions);

            services.AddAssemblyType(typeof(IInvoke), Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyType(typeof(IChannel), Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyType(typeof(ISagaState), Assembly.GetEntryAssembly()!, true);
            services.AddAssemblyType(typeof(ISagaTrigger), Assembly.GetEntryAssembly()!, true);
            services.AddAssemblyType(typeof(ISagaEvent), Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyTypeByInterface<ISubscribe>(Assembly.GetEntryAssembly()!, false);
            services.AddAssemblyTypeByInterface<IInitialize>(Assembly.GetEntryAssembly()!, true);
            services.AddAssemblyTypeByInterface<IInvokeFactory>(Assembly.GetEntryAssembly()!, true);
        }

        public static void AddPanamaCanal(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly> assemblies, Action<CanalOptions>? setup = null)
        {
            AddPanamaCanalBase(services, config);

            var settings = new MemorySettings();
            config.GetSection("MemorySettings").Bind(settings);

            var options = new CanalOptions();
            config.GetSection(CanalOptions.Section).Bind(options);

            if (setup != null)
                setup(options);

            services.AddSingleton(options);
            services.AddSingleton(settings);

            services.AddAssemblyTypes<IInvoke>(assemblies.Distinct(), false);
            services.AddAssemblyTypes<IChannel>(assemblies.Distinct(), false);
            services.AddAssemblyTypes<IInitialize>(assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaState>(assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaTrigger>(assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaEvent>(assemblies.Distinct(), false);
            services.AddAssemblyTypesByInterface<ISubscribe>(assemblies.Distinct(), false);
            services.AddAssemblyTypesByInterface<IInitialize>(assemblies.Distinct(), true);
            services.AddAssemblyTypesByInterface<IInvokeFactory>(assemblies.Distinct(), true);
        }
    }
}
