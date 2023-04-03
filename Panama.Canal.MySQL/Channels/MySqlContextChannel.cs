using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.MySQL.Channels
{
    public class MySqlContextChannel : DefaultChannel, IChannel<DatabaseFacade, IDbContextTransaction>
    {
        private IDbContextTransaction? _channel = null;

        private readonly ILogger<MySqlChannel> _log;
        private readonly IServiceProvider _provider;

        public IDbContextTransaction? Current => _channel;

        public MySqlContextChannel(
              IDispatcher dispatcher
            , IServiceProvider provider
            , ILogger<MySqlChannel> log)
            : base(log, dispatcher, provider)
        {
            _log = log;
            _provider = provider;
        }

        public override async Task Commit(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            _channel?.Commit();

            await base.Commit(token).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            _channel?.Dispose();

            base.Dispose();
        }

        public void Open(DatabaseFacade channel, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
           
            _channel = channel.BeginTransactionAsync().GetAwaiter().GetResult();
        }

        public override void Rollback(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            _channel?.Rollback();
        }

        public override async Task Post<T, I>(string name, string? ack = null, string? nack = null, string? group = null, DateTime? delay = null, string? instance = null, string? correlationId = null, CancellationToken token = default, IDictionary<string, string?>? headers = null, params IModel[]? data)
        {
            token.ThrowIfCancellationRequested();

            var result = await _provider.GetRequiredService<IBus>()
                .Instance(instance)
                .Transaction(Current)
                .Header(headers)
                .Token(token)
                .Id(Guid.NewGuid().ToString())
                .CorrelationId(correlationId)
                .Topic(name)
                .Group(group)
                .Data(data)
                .Ack(ack)
                .Nack(nack)
                .Invoker<I>()
                .Target<T>()
                .Delay(delay)
                .Post();

            Queue.EnqueueResult(result);
        }
    }
}
