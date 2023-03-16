using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Quartz;
using Quartz.Spi;

namespace Panama.Canal
{
    public static class Registrar
    {
        public static void AddPanamaCanal(this IServiceCollection services)
        {
            services.AddHostedService<Bootstrapper>();
            //services.AddSingleton<IJobFactory, SingletonFactory>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<IInvokeBrokers, Brokers>();
            services.AddSingleton<IInvokeSubscriptions, Subscriptions>();

            services.AddSingleton<Dispatcher>();
            services.AddSingleton<PublishedRetry>();
            services.AddSingleton<ReceivedRetry>();
            services.AddSingleton<DeleteExpired>();

            services.AddSingleton(new Job(
                type: typeof(Dispatcher),
                expression: "0/1 * * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(PublishedRetry),
                expression: "0/5 * * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(ReceivedRetry),
                expression: "0/5 * * * * ?"));
            services.AddSingleton(new Job(
                type: typeof(DeleteExpired),
                expression: "0/5 * * * * ?"));

            services.AddQuartz(q => {
                q.SchedulerName = "panama-canal-services";
                q.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddQuartzHostedService(
                q => q.WaitForJobsToComplete = true);
        }
    }
}
