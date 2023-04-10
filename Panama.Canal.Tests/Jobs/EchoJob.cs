using Quartz;

namespace Panama.Canal.Tests.Jobs
{
    [DisallowConcurrentExecution]
    public class EchoJob : IJob
    {
        private readonly IServiceProvider _provider;

        public EchoJob(
              IServiceProvider provider)
        {
            _provider = provider;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            await Console.Out.WriteLineAsync("Greetings from EchoJob!");
        }
    }
}