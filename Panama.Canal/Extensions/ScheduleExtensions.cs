using Panama.Canal.Models;
using Quartz;

namespace Panama.Canal.Extensions
{
    public static class ScheduleExtensions
    {
        public static string GetKey(this Job schedule)
        {
            var name = $"{schedule.Type.FullName}";

            return name;
        }

        public static JobKey GetJobKey(this Job schedule)
        {
            var name = $"{schedule.GetKey()}-job";
         
            return new JobKey(name, schedule.Group);
        }

        public static TriggerKey GetTriggerKey(this Job schedule)
        {
            var name = $"{schedule.GetKey()}-trigger";

            return new TriggerKey(name);
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
            var trigger = TriggerBuilder.Create()
                .ForJob(schedule.GetJobKey())
                .WithIdentity(schedule.GetTriggerKey())
                .WithCronSchedule(schedule.CronExpression
                    , x => x.WithMisfireHandlingInstructionDoNothing())
                .WithDescription(schedule.CronExpression)
                .Build();

            return trigger;
        }

        public static TimeSpan GetScheduledInterval(this Job schedule)
        {
            var cron = new CronExpression(schedule.CronExpression);
            
            var interval = cron.GetNextValidTimeAfter(DateTime.UtcNow) - DateTime.UtcNow;
            if (interval == null)
                throw new InvalidOperationException($"Scheduled interval for job schedule: {schedule.GetKey()} cannot be parsed: {schedule.CronExpression}");

            return interval.Value;
        }
    }
}
