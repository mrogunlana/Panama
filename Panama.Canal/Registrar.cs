using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Quartz;

namespace Panama.Canal
{
    public static class Registrar
    {
        public static void AddPanamaCanal(this IServiceCollection services)
        {
            services.AddHostedService<Bootstrapper>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<IInvokeBrokers, Brokers>();
            services.AddSingleton<IInvokeSubscriptions, Subscriptions>();

            services.AddQuartz(q =>
            {
                q.SchedulerName = "Panama Canal Services";
                q.UseMicrosoftDependencyInjectionJobFactory();
                
                q.AddCanalJob<Dispatcher>();
                q.AddCanalJob<PublishedRetry>(cronTrigger: "0/5 * * * * ?");
                q.AddCanalJob<ReceivedRetry>(cronTrigger: "0/5 * * * * ?");
                q.AddCanalJob<DeleteExpired>(cronTrigger: "0/5 * * * * ?");
            });

            services.AddQuartzHostedService(
                q => q.WaitForJobsToComplete = true);
        }
    }
}
