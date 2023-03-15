using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteExpired : IJob
    {
        public DeleteExpired(IServiceProvider provider)
        {
            
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}