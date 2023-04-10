using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Models;
using Quartz;

namespace Panama.Canal.Tests.Jobs
{
    [DisallowConcurrentExecution]
    public class PublishOutbox : IJob
    {
        private readonly Store _store;
        private readonly IProcessorFactory _factory;
        private readonly IServiceProvider _provider;

        public PublishOutbox(
            Store store,
            IServiceProvider provider,
            IProcessorFactory factory)
        {
            _store = store;
            _factory = factory;
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var outbox = _store.Outbox.Take(200).Select(x => x).AsEnumerable();

            foreach (var publish in outbox)
            {
                await _factory
                    .GetProcessor(publish.Value)
                    .Execute(new Context()
                        .Add(publish.Value)
                        .Token(context.CancellationToken));

                _store.Outbox.TryRemove(publish.Key, out _);
            }
        }
    }
}