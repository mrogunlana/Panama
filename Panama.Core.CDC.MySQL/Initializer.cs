using Microsoft.Extensions.Options;
using Panama.Core.CDC.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class Initializer : IInitialize
    {
        private readonly IOptions<MySqlCdcOptions> _options;
        private readonly IServiceProvider _provider;
        private readonly IStore _store;

        public Initializer(
             IStore store
           , IServiceProvider provider
           , IOptions<MySqlCdcOptions> options)
        {
            _store = store;
            _options = options;
            _provider = provider;
        }
        public async Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            await _store.InitLocks();
        }
    }
}