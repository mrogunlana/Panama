using Microsoft.Extensions.Options;
using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class Initializer : IInitialize
    {
        private readonly IOptions<MySqlOptions> _options;
        private readonly IServiceProvider _provider;
        private readonly IStore _store;
        
        private MySqlSettings _settings = default!;

        public Initializer(
             IStore store
           , IServiceProvider provider
           , IOptions<MySqlOptions> options)
        {
            _store = store;
            _options = options;
            _provider = provider;
        }

        public IModel Settings => _settings;

        public async Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            //1. Initialize Settings
            var settings = new MySqlSettings();

            settings.PublishedTable = $"{_options.Value.TablePrefix}.Published";
            settings.ReceivedTable = $"{_options.Value.TablePrefix}.Received";
            settings.LockTable = $"{_options.Value.TablePrefix}.Lock";
            settings.PublishedTableId = await _store.GetPublishedTableId().ConfigureAwait(false);
            settings.ReceivedTableId = await _store.GetReceivedTableId().ConfigureAwait(false);
            settings.PublishedTableMap = await _store.GetPublishedSchema().ConfigureAwait(false);
            settings.ReceivedTableMap = await _store.GetReceivedSchema().ConfigureAwait(false);

            _settings = settings;

            //2. Initialize Database Store
            await _store.Init().ConfigureAwait(false);
        }
    }
}