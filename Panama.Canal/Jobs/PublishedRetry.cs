using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class PublishedRetry : IJob
    {
        public PublishedRetry(IServiceProvider provider)
        {
            
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}