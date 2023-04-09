using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Security;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System.Reflection;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class JobTests
    {
        private IServiceProvider _provider;

        public JobTests()
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging();
            services.AddSingleton<IServiceCollection>(_ => services);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);

            var assemblies = new List<Assembly>();

            // domain built like so to overcome .net core .dll discovery issue 
            // within container:
            assemblies.Add(Assembly.GetExecutingAssembly());
            assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            assemblies.AddRange(Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(x => Assembly.Load(x))
                .ToList());

            var domain = assemblies.ToArray();

            services.AddPanama(domain);
            services.AddPanamaCanal(configuration, domain);
            services.AddPanamaSecurity();

            _provider = services.BuildServiceProvider();
        }

        private async Task<(IJob, IJobExecutionContext)> BuildJob<J>()
            where J : IJob
        {
            var source = new CancellationTokenSource();
            var manifest = _provider.GetRequiredService<IEnumerable<Job>>();
            var stage = manifest.Where(x => x.Type == typeof(J)).FirstOrDefault();

            Assert.IsNotNull(stage);

            var job = (J)_provider.GetRequiredService(stage.Type);
            var schedules = _provider.GetRequiredService<ISchedulerFactory>();
            var schedule = await schedules
                .GetScheduler(source.Token)
                .ConfigureAwait(false);

            var detail = stage.CreateJob();
            var trigger = stage.CreateTrigger();

            var bundle = new TriggerFiredBundle(
                job: detail,
                trigger: new SimpleTriggerImpl(),
                cal: new WeeklyCalendar(),
                jobIsRecovering: false,
                fireTimeUtc: DateTimeOffset.UtcNow,
                scheduledFireTimeUtc: DateTimeOffset.UtcNow,
                prevFireTimeUtc: DateTimeOffset.UtcNow,
                nextFireTimeUtc: DateTimeOffset.UtcNow);

            var context = new JobExecutionContextImpl(schedule, bundle, job);

            return (job, context);
        }

        [TestMethod]
        public async Task VerifyBootstrapper()
        {
            var source = new CancellationTokenSource();
            var bootstraper = _provider.GetRequiredService<IBootstrapper>();

            await bootstraper.On(source.Token);

            Assert.IsTrue(bootstraper.Online);

            await bootstraper.Off(source.Token);

            Assert.IsFalse(bootstraper.Online);
        }

        [TestMethod]
        public async Task VerifyReceivedRetry()
        {
            var (job, context) = await BuildJob<ReceivedRetry>();

            await job.Execute(context);
        }

        [TestMethod]
        public async Task VerifyDeleteExpired()
        {
            var (job, context) = await BuildJob<DeleteExpired>();

            await job.Execute(context);
        }

        [TestMethod]
        public async Task VerifyPublishedRetry()
        {
            var (job, context) = await BuildJob<PublishedRetry>();

            await job.Execute(context);
        }

        [TestMethod]
        public async Task VerifyDelayedPublished()
        {
            var (job, context) = await BuildJob<DelayedPublished>();

            await job.Execute(context);
        }
    }
}