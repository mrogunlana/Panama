using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class ReceivedRetry : IJob
    {
        private readonly Job _job;
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;
        private readonly IOptions<CanalOptions> _options;

        public ReceivedRetry(
              IStore store
            , IEnumerable<Job> jobs
            , IDispatcher dispatcher
            , IOptions<CanalOptions> options)
        {
            _store = store;
            _options = options;
            _dispatcher = dispatcher;

            _job = jobs.Where(x => x.Type == typeof(ReceivedRetry)).First();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var interval = _job.GetScheduledInterval();
            var ttl = interval.Add(TimeSpan.FromSeconds(10));

            if (_options.Value.UseLock)
                if (!await _store.AcquireReceivedRetryLock(ttl, token: context.CancellationToken))
                    return;

            var retry = await _store.GetReceivedMessagesToRetry().ConfigureAwait(false); ;

            foreach (var received in retry)
                await _dispatcher.Execute(received, context.CancellationToken).ConfigureAwait(false);

            if (_options.Value.UseLock)
                await _store.ReleaseReceivedLock(token: context.CancellationToken);
        }
    }
}