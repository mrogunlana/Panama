using Panama.Canal.Interfaces;
using Quartz;

namespace Panama.Canal.Tests.Jobs
{
    [DisallowConcurrentExecution]
    public class ReceiveInbox : IJob
    {
        private readonly Store _store;
        private readonly IDispatcher _dispatcher;

        public ReceiveInbox(
              Store store
            , IDispatcher dispatcher)
        {
            _store = store;
            _dispatcher = dispatcher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var inbox = _store.Inbox.Take(200).Select(x => x).AsEnumerable();

            foreach (var received in inbox)
            {
                await _dispatcher.Execute(received.Value, context.CancellationToken).ConfigureAwait(false);

                _store.Inbox.TryRemove(received.Key, out _);
            }
        }
    }
}