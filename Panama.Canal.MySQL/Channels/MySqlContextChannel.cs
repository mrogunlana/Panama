using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;

namespace Panama.Canal.MySQL.Channels
{
    public class MySqlContextChannel : DefaultChannel, IChannel<DatabaseFacade, IDbContextTransaction>
    {
        private readonly ILogger<MySqlChannel> _log;
        private readonly IServiceProvider _provider;

        public MySqlContextChannel(
              IProcessorFactory factory
            , IServiceProvider provider
            , ILogger<MySqlChannel> log)
            : base(factory, provider)
        {
            _log = log;
            _provider = provider;
        }

        public override async Task Commit(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Current?.To<IDbContextTransaction>()?.Commit();

            await base.Commit(token).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            Current?.To<IDbContextTransaction>()?.Dispose();

            base.Dispose();
        }

        public void Open(DatabaseFacade channel, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Current = channel.BeginTransactionAsync().GetAwaiter().GetResult();
        }

        public override void Rollback(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Current?.To<IDbContextTransaction>()?.Rollback();
        }
    }
}
