using Microsoft.Extensions.Options;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Extensions;
using Panama.Models;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class PublishedRetry : IJob
    {
        private readonly Job _job;
        private readonly IStore _store;
        private readonly IOptions<CanalOptions> _options;
        private readonly IProcessorFactory _factory;

        public PublishedRetry(
              IStore store
            , IEnumerable<Job> jobs
            , IProcessorFactory factory
            , IOptions<CanalOptions> options)
        {
            _store = store;
            _options = options;
            _factory = factory;

            _job = jobs.Where(x => x.Type == typeof(PublishedRetry)).First();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var interval = _job.GetScheduledInterval();
            var ttl = interval.Add(TimeSpan.FromSeconds(10));

            if (_options.Value.UseLock)
                if (!await _store.AcquirePublishedRetryLock(ttl, token: context.CancellationToken))
                    return;

            var retry = await _store.GetPublishedMessagesToRetry().ConfigureAwait(false);

            foreach (var publish in retry)
                await _factory
                    .GetProcessor(publish)
                    .Execute(new Context()
                        .Add(retry)
                        .Token(context.CancellationToken));

            if (_options.Value.UseLock)
                await _store.ReleasePublishedLock(token: context.CancellationToken);
        }
    }
}