using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Panama.Canal.Models;
using Panama.Security.Resolvers;
using Quartz;

namespace Panama.Canal.Extensions
{
    internal static class ScheduleExtensions
    {
        public static JobKey GetJobKey(this Job schedule)
        {
            var name = $"{schedule.Type.FullName}-job";
         
            return new JobKey(name, schedule.Group);
        }

        public static IJobDetail CreateJob(this Job schedule)
        {
            var key = schedule.GetJobKey();
            var jobBuilder = JobBuilder.Create(schedule.Type);

            jobBuilder.WithIdentity(key);

            var job = jobBuilder
                .WithDescription(schedule.Type.Name)
                .Build();

            return job;
        }

        public static ITrigger CreateTrigger(this Job schedule)
        {
            var key = schedule.GetJobKey();

            var trigger = TriggerBuilder.Create()
                .ForJob(key)
                .WithIdentity($"{schedule.Type.FullName}.trigger")
                .WithCronSchedule(schedule.CronExpression)
                .WithDescription(schedule.CronExpression)
                .Build();

            return trigger;
        }
    }
}
