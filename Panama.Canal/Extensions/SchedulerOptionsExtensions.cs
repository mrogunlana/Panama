using Panama.Canal.Models;
using Panama.Canal.Models.Options;
using Quartz;
using System.Collections.Generic;

namespace Panama.Canal.Extensions
{
    public static class SchedulerOptionsExtensions
    {
        public static SchedulerOptions AddJob<J>(this SchedulerOptions options, string cronExpression)
            where J : IJob
        {
            options.Added.Add(new Job(
                type: typeof(J),
                expression: cronExpression));

            return options;
        }

        public static SchedulerOptions RemoveJob<J>(this SchedulerOptions options)
            where J : IJob
        {
            options.Removed.Add(typeof(J));

            return options;
        }

        public static IEnumerable<Job> GetJobs(this SchedulerOptions options)
        {
            var results = options.Current.ToList();

            var removed = results.Where(j => options.Removed.Contains(j.Type)).ToList();

            foreach (var remove in removed)
                results.Remove(remove);

            foreach (var add in options.Added)
                results.Add(add);

            return results;
        }
    }
}