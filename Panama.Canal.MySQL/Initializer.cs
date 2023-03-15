using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.MySQL
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
            settings.OutboxTable = $"{_options.Value.TablePrefix}.Outbox";
            settings.InboxTable = $"{_options.Value.TablePrefix}.Inbox";

            //2. Initialize Database Store
            await _store.Init().ConfigureAwait(false);

            //3. Set Table IDs and Table Maps
            settings.PublishedTableId = await _store.GetTableId(settings.PublishedTable).ConfigureAwait(false);
            settings.ReceivedTableId = await _store.GetTableId(settings.ReceivedTable).ConfigureAwait(false);
            settings.OutboxTableId = await _store.GetTableId(settings.OutboxTable).ConfigureAwait(false);
            settings.InboxTableId = await _store.GetTableId(settings.InboxTable).ConfigureAwait(false);
            settings.PublishedTableMap = await _store.GetSchema(settings.PublishedTable).ConfigureAwait(false);
            settings.ReceivedTableMap = await _store.GetSchema(settings.ReceivedTable).ConfigureAwait(false);
            settings.OutboxTableMap = await _store.GetSchema(settings.OutboxTable).ConfigureAwait(false);
            settings.InboxTableMap = await _store.GetSchema(settings.InboxTable).ConfigureAwait(false);

            _settings = settings;

            
        }
    }
}