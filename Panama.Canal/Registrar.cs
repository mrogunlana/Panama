using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddHostedService<Bootstrapper>();

            services.AddTransient<IProcessor, DefaultProcessor>();
            services.AddTransient<IProcessor, SagaProcessor>();
            services.AddSingleton<IProcessorFactory, ProcessorFactory>();

            services.AddTransient<IBus, Bus>();
            services.AddTransient<IInvoke, PollingPublisherInvoker>();
            services.AddTransient<IInvoke, OutboxInvoker>();
            services.AddTransient<IInvoke, SubscriptionInvoker>();
            services.AddTransient<IInvoke, BrokerInvoker>();
            services.AddTransient<IChannel, DefaultChannel>();
            services.AddTransient<IDefaultChannelFactory, DefaultChannelFactory>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<ITarget, DefaultTarget>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            services.AddSingleton<IStore, Store>();
            services.AddSingleton<ISagaFactory, StatelessSagaFactory>();
            services.AddSingleton<ISagaTriggerFactory, StatelessSagaTriggerFactory>();
            services.AddSingleton<ISagaStateFactory, StatelessSagaStateFactory>();

            var settings = new MemorySettings();
            config.GetSection("MemorySettings").Bind(settings);

            services.AddSingleton(settings);
            services.AddSingleton<MemorySettings>();
            services.AddSingleton<ReceivedRetry>();
            services.AddSingleton<DeleteExpired>();
            services.AddSingleton<PublishedRetry>();
            services.AddSingleton<DelayedPublished>();
            
            services.AddSingleton(new Job(
                type: typeof(Dispatcher),
                expression: "0/1 * * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(DelayedPublished),
                expression: "0/0 1 * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(PublishedRetry),
                expression: "0/0 1 * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(ReceivedRetry),
                expression: "0/0 1 * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(DeleteExpired),
                expression: "0/0 5 * * * ?"));

            services.AddQuartz(q => {
                q.SchedulerName = "panama-canal-services";
                q.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(
                q => q.WaitForJobsToComplete = true);

            services.Configure<CanalOptions>(options =>
                config.GetSection(CanalOptions.Section).Bind(options));

        }
        public static void AddPanamaCanal(this IServiceCollection services, IConfiguration config)
        {
            AddPanamaCanalBase(services, config);

            services.AddAssemblyType(typeof(ISubscribe), Assembly.GetEntryAssembly()!, false);
        }

        public static void AddPanamaCanal(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly> assemblies)
        {
            AddPanamaCanalBase(services, config);

            services.AddAssemblyTypes<ISubscribe>(assemblies.Distinct(), false);
            services.AddAssemblyTypes<ISagaState>(assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaTrigger>(assemblies.Distinct(), true);
            services.AddAssemblyTypes<ISagaEvent>(assemblies.Distinct(), false);
        }
    }
}
