using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Extensions;
using Panama.Models;
using Quartz;

namespace Panama.Canal.Jobs
{
    [DisallowConcurrentExecution]
    public class DelayedPublished : IJob
    {
        private readonly IStore _store;
        private readonly IProcessorFactory _factory;

        public DelayedPublished(
              IStore store
            , IProcessorFactory factory)
        {
            _store = store;
            _factory = factory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            async Task Dispatch(object transaction, IEnumerable<InternalMessage> messages)
            {
                foreach (var message in messages)
                    await _factory
                        .GetProducerProcessor(message)
                        .Execute(new Context()
                            .Add(message)
                            .Add(new Kvp<string, DateTime?>("Delay", message.Expires))
                            .Token(context.CancellationToken));
            }

            await _store.GetDelayedPublishedMessagesForScheduling(Dispatch, token: context.CancellationToken);
        }
    }
}