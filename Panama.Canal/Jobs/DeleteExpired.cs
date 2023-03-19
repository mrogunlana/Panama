using Microsoft.Extensions.Logging;
using Panama.Canal.Interfaces;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteExpired : IJob
    {
        private readonly IStore _store;

        public DeleteExpired(IStore store)
        {
            _store = store;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var date = DateTime.UtcNow;

            await _store.DeleteExpiredPublishedAsync(date, token: context.CancellationToken);
            await _store.DeleteExpiredOutboxAsync(date, token: context.CancellationToken);
            await _store.DeleteExpiredInboxAsync(date, token: context.CancellationToken);
            await _store.DeleteExpiredOutboxAsync(date, token: context.CancellationToken);
        }
    }
}