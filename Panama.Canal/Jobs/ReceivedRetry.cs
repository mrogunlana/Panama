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
        private readonly IOptions<CanalOptions> _options;
        private readonly IInvokeSubscriptions _subscriptions;

        public ReceivedRetry(
              IStore store
            , IEnumerable<Job> jobs
            , IOptions<CanalOptions> options
            , IInvokeSubscriptions subscriptions)
        {
            _store = store;
            _options = options;
            _subscriptions = subscriptions;

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

            //TODO: publish to message subscribers once jobs are impl
            //foreach (var broker in _brokers)
            //    foreach (var publish in retry)
            //        await broker.Publish(new MessageContext(publish, token: context.CancellationToken));

            if (_options.Value.UseLock)
                await _store.ReleaseReceivedLock(token: context.CancellationToken);
        }
    }
}