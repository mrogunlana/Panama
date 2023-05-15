using Quartz;

namespace Panama.Canal.Extensions
{
    public static class QuartzExtensions
    {
        public static IServiceCollectionQuartzConfigurator AddCanalJob<T>(this IServiceCollectionQuartzConfigurator quartz, string? name = null, string? cronTrigger = null)
            where T : IJob
        {
            var _name = name ?? $"{typeof(T).Name}-job";
            var key = new JobKey(_name);

            quartz.AddJob<T>(opts => opts.WithIdentity(key));

            quartz.AddTrigger(opts => opts
                .ForJob(key) 
                .WithIdentity($"{_name}-trigger") 
                .WithCronSchedule(cronTrigger ?? "0/1 * * * * ?")); 

            return quartz;
        }
    }
}
