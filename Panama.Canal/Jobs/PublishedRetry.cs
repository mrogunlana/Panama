using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class PublishedRetry : IJob
    {
        private readonly Job _job;
        private readonly IStore _store;
        private readonly IInvokeBrokers _brokers;
        private readonly IOptions<CanalOptions> _options;

        public PublishedRetry(
              IStore store
            , IEnumerable<Job> jobs
            , IInvokeBrokers brokers
            , IOptions<CanalOptions> options)
        {
            _store = store;
            _options = options;
            _brokers = brokers;

            _job = jobs.Where(x => x.Type == typeof(PublishedRetry)).First();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var interval = _job.GetScheduledInterval();
            var ttl = interval.Add(TimeSpan.FromSeconds(10));

            if (_options.Value.UseLock)
                if (!await _store.AcquirePublishedRetryLock(ttl, token: context.CancellationToken))
                    return;

            var retry = await _store.GetPublishedMessagesToRetry().ConfigureAwait(false); ;

            //TODO: publish to message brokers once jobs are impl
            //foreach (var broker in _brokers)
            //    foreach (var publish in retry)
            //        await broker.Publish(new MessageContext(publish, token: context.CancellationToken));

            if (_options.Value.UseLock)
                await _store.ReleasePublishedLock(token: context.CancellationToken);
        }
    }
}