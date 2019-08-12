using Panama.Core.Logger;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using External = Quartz;

namespace Panama.Core.Jobs.Quartz
{
    public class Scheduler : IScheduler
    {
        private readonly External.IScheduler _scheduler;
        private readonly ILog _log;

        public Scheduler(ILog log)
        {
            _scheduler = StdSchedulerFactory
                .GetDefaultScheduler()
                .ConfigureAwait(true)
                .GetAwaiter()
                .GetResult();
            
            _log = log;
        }
        public void Start()
        {
            _scheduler.Start();

            _log.LogInformation<Scheduler>($"Scheduler started. Running jobs every minute.");
        }

        public void Stop()
        {
            _scheduler.Shutdown();

            _log.LogInformation<Scheduler>($"Scheduler shutdown.");
        }

        public void Queue<T>(T job, int minutes)
        {
            var name = job.GetType().Name;

            ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"{name}_Every{minutes}Minutes", "Group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(minutes)
                        .RepeatForever())
                    .Build();

            //note: this breaks if T not IJob type...
            IJobDetail detail = JobBuilder.Create(typeof(T))
                    .WithIdentity($"{name}_Job", "Group1")
                    .Build();
            
            _scheduler.ScheduleJob(detail, trigger);
        }

        public void Queue<T>(T job)
        {
            var name = job.GetType().Name;

            ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"{name}_EveryMinute", "Group1")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever())
                    .Build();

            //note: this breaks if T not IJob type...
            IJobDetail detail = JobBuilder.Create(typeof(T))
                    .WithIdentity($"{name}_Job", "Group1")
                    .Build();

            _scheduler.ScheduleJob(detail, trigger);
        }

        public int Count()
        {
            if (_scheduler == null)
                return 0;

            var keys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Group1"));
            if (keys == null)
                return 0;

            return keys
                .ConfigureAwait(true)
                .GetAwaiter()
                .GetResult()
                .Count;
        }
    }
}
