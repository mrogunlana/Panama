using Microsoft.Extensions.DependencyInjection;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.MySQL.Extensions;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class Initializer : IInitialize
    {
        private readonly MySqlCdcOptions _settings;

        public Initializer(IServiceProvider provider)
        {
            _settings = provider.GetService<MySqlCdcOptions>()!;
        }
        public async Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            await _settings.InitLocks();
        }
    }
}