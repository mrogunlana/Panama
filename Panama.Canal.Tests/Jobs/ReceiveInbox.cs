using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Models;
using Quartz;

namespace Panama.Canal.Tests.Jobs
{
    [DisallowConcurrentExecution]
    public class ReceiveInbox : IJob
    {
        private readonly Store _store;
        private readonly IProcessorFactory _factory;
        public ReceiveInbox(
              Store store
            , IProcessorFactory factory)
        {
            _store = store;
            _factory = factory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var inbox = _store.Inbox.Take(200).Select(x => x).AsEnumerable();

            foreach (var received in inbox)
            {
                await _factory
                    .GetConsumerProcessor(received.Value)
                    .Execute(new Context()
                        .Add(received.Value)
                        .Token(context.CancellationToken))
                    .ConfigureAwait(false);

                _store.Inbox.TryRemove(received.Key, out _);
            }
        }
    }
}