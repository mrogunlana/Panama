using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Jobs;
using Panama.Canal.Models;
using Panama.Canal.Tests.Jobs;
using Panama.Security;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

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

            services.AddPanama(
                configuration: configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            //add custom jobs to process outbox/inbox messages:
            services.AddSingleton<EchoJob>();
            services.AddSingleton<PublishOutbox>();
            services.AddSingleton<ReceiveInbox>();
            services.AddSingleton(new Job(
                type: typeof(EchoJob),
                expression: "* * * ? * *"));
            services.AddSingleton(new Job(
                type: typeof(PublishOutbox),
                expression: "* * * ? * *"));
            services.AddSingleton(new Job(
                type: typeof(ReceiveInbox),
                expression: "* * * ? * *"));

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

        private async Task<IEnumerable<IJobDetail>> GetActiveJobs(Quartz.IScheduler scheduler)
        {
            // Get the resulting job that should be running
            var result = new List<IJobDetail>();
            var keys = await scheduler.GetJobGroupNames();
            foreach (var key in keys)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(key);
                var jobKeys = await scheduler.GetJobKeys(groupMatcher);
                foreach (var jobKey in jobKeys)
                {
                    var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);

                    if (detail == null)
                        continue;
                    if (triggers.Count > 0)
                        result.Add(detail);
                }
            }

            return result;
        }

        [TestMethod]
        public async Task VerifyBootstrapper()
        {
            var source = new CancellationTokenSource();
            var bootstraper = _provider.GetRequiredService<IBootstrapper>();
            var scheduler = _provider.GetRequiredService<Interfaces.IScheduler>();

            await bootstraper.On(source.Token);

            Assert.IsTrue(bootstraper.Online);

            var result = await GetActiveJobs(scheduler.Current!);

            await bootstraper.Off(source.Token);

            Assert.IsFalse(bootstraper.Online);
            Assert.AreEqual(result.Count(), 7);
        }

        [TestMethod]
        public async Task VerifyQuartzScheduling()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = await schedulerFactory.GetScheduler();
            var jobFactory = _provider.GetRequiredService<IJobFactory>();

            scheduler.JobFactory = jobFactory;

            // and start it off
            await scheduler.Start();
            
            // define the job and tie it to our Echo 
            IJobDetail job = JobBuilder.Create<PublishOutbox>()
                .WithIdentity("job1", "group1")
                .Build();

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(1)
                    .RepeatForever())
                .Build();

            // Tell Quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);

            // Set some time to allow jobs schedules to start
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Get the resulting job that should be running
            var result = await GetActiveJobs(scheduler);

            // and last shut down the scheduler when you are ready to close your program
            await scheduler.Shutdown();

            Assert.AreEqual(result.Count(), 1);
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