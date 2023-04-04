using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Interfaces;
using System.Data;

namespace Panama.Canal.MySQL.Channels
{
    public class MySqlChannel : DefaultChannel, IChannel<IDbConnection, IDbTransaction>
    {
        private readonly ILogger<MySqlChannel> _log;
        private readonly IServiceProvider _provider;

        public MySqlChannel(
              IProcessorFactory factory
            , IServiceProvider provider
            , ILogger<MySqlChannel> log)
            : base(log, factory, provider)
        {
            _log = log;
            _provider = provider;
        }

        public override async Task Commit(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Current?.To<IDbTransaction>()?.Commit();

            await base.Commit(token).ConfigureAwait(false);
        }

        public override void Dispose()
        {
            Current?.To<IDbTransaction>()?.Dispose();

            base.Dispose();
        }

        public void Open(IDbConnection channel, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            if (channel.State == ConnectionState.Closed) 
                channel.Open();

            Current = channel.BeginTransaction();
        }

        public override void Rollback(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Current?.To<IDbTransaction>()?.Rollback();
        }
    }
}
