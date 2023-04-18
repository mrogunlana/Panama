using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Models.Markers;
using Panama.Canal.Models.Options;
using Panama.Extensions;
using Panama.Interfaces;
using Quartz;

namespace Panama.Canal.Registrars
{
    public class Scheduler : IRegistrar
    {
        private readonly Panama.Models.Builder _builder;
        private readonly Action<SchedulerOptions> _setup;

        public Type Marker => typeof(SchedulerMarker);

        public Scheduler(
            Panama.Models.Builder builder,
            Action<SchedulerOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }
        
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(new SchedulerMarker());

            services.AddHostedService<Canal.Scheduler>();
            services.AddSingleton<Interfaces.IScheduler, Canal.Scheduler>();

            //services.AddSingleton<ReceivedRetry>();
            //services.AddSingleton<DeleteExpired>();
            //services.AddSingleton<PublishedRetry>();
            //services.AddSingleton<DelayedPublished>();

            //services.AddSingleton(new Job(
            //    type: typeof(DelayedPublished),
            //    expression: "0 */1 * ? * *"));
            //services.AddSingleton(new Job(
            //    type: typeof(PublishedRetry),
            //    expression: "0 */1 * ? * *"));
            //services.AddSingleton(new Job(
            //    type: typeof(ReceivedRetry),
            //    expression: "0 */1 * ? * *"));
            //services.AddSingleton(new Job(
            //    type: typeof(DeleteExpired),
            //    expression: "0 */5 * ? * *"));

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
        }

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;

            var options = new SchedulerOptions();

            options.SetBuilder(new Panama.Models.Builder(_builder.Configuration, _builder.Assemblies));

            _setup(options);

            var jobs = options.GetJobs();

            foreach (var job in jobs)
            {
                services.AddSingleton(job.Type);
                services.AddSingleton(job);
            }

            services.Configure(_setup);
        }
    }
}
