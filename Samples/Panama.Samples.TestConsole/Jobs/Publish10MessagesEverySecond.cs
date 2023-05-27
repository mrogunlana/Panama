using Quartz;

namespace Panama.Samples.TestConsole.Jobs
{
    [DisallowConcurrentExecution]
    public class Publish10MessagesEverySecond : IJob
    {
        private readonly IServiceProvider _provider;

        public Publish10MessagesEverySecond(
              IServiceProvider provider)
        {
            _provider = provider;
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}