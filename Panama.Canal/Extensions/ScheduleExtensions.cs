using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Security.Resolvers;
using Quartz;

namespace Panama.Canal.Extensions
{
    internal static class ScheduleExtensions
    {
        public static IJobDetail CreateJob(this Schedule schedule)
        {
            var jobBuilder = JobBuilder.Create(schedule.JobType);

            if (string.IsNullOrEmpty(schedule.Group))
                jobBuilder.WithIdentity($"{schedule.JobType.FullName}");
            else
                jobBuilder.WithIdentity($"{schedule.JobType.FullName}", schedule.Group);

            var job = jobBuilder
                .WithDescription(schedule.JobType.Name)
                .Build();

            return job;
        }

        public static ITrigger CreateTrigger(this Schedule schedule)
        {
            var triggerBuilder = TriggerBuilder.Create();
            if (string.IsNullOrEmpty(schedule.Group))
                triggerBuilder.WithIdentity($"{schedule.JobType.FullName}.trigger");
            else
                triggerBuilder.WithIdentity($"{schedule.JobType.FullName}.trigger", schedule.Group);

            var trigger = triggerBuilder
                .WithCronSchedule(schedule.CronExpression)
                .WithDescription(schedule.CronExpression)
                .Build();

            return trigger;
        }
    }
}
