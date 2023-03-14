using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class ReceivedRetry : IJob
    {
        public ReceivedRetry(IServiceProvider provider)
        {

        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}