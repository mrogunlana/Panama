using Microsoft.Extensions.Logging;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class DelayedPublished : IJob
    {
        private readonly IStore _store;
        private readonly IDispatcher _dispatcher;

        public DelayedPublished(
              IStore store
            , IDispatcher dispatcher)
        {
            _store = store;
            _dispatcher = dispatcher; 
        }

        public async Task Execute(IJobExecutionContext context)
        {
            async Task Dispatch(object transaction, IEnumerable<InternalMessage> messages)
            {
                foreach (var message in messages)
                    await _dispatcher.Schedule(message, message.Expires!.Value, transaction).ConfigureAwait(false);
            }

            await _store.GetDelayedPublishedMessagesForScheduling(Dispatch, token: context.CancellationToken);
        }
    }
}