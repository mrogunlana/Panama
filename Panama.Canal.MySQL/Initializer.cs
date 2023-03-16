﻿using Microsoft.Extensions.Options;
using Panama.Canal.Interfaces;

namespace Panama.Canal.MySQL
{
    public class Initializer : IInitialize
    {
        private readonly IStore _store;
        private readonly IOptions<MySqlOptions> _options;
        private readonly IServiceProvider _provider;
        
        private MySqlSettings _settings = default!;

        public Initializer(
             IStore store
           , IServiceProvider provider
           , MySqlSettings settings
           , IOptions<MySqlOptions> options)
        {
            _store = store;
            _options = options;
            _settings = settings;
            _provider = provider;
        }

        public async Task Invoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            //1. Initialize Settings
            _settings.PublishedTable = $"{_options.Value.TablePrefix}.Published";
            _settings.ReceivedTable = $"{_options.Value.TablePrefix}.Received";
            _settings.LockTable = $"{_options.Value.TablePrefix}.Lock";
            _settings.OutboxTable = $"{_options.Value.TablePrefix}.Outbox";
            _settings.InboxTable = $"{_options.Value.TablePrefix}.Inbox";

            //2. Initialize Database Store
            await _store.Init();

            //3. Set Table IDs and Table Maps
            _settings.PublishedTableId = await _store.GetTableId(_settings.PublishedTable).ConfigureAwait(false);
            _settings.ReceivedTableId = await _store.GetTableId(_settings.ReceivedTable).ConfigureAwait(false);
            _settings.OutboxTableId = await _store.GetTableId(_settings.OutboxTable).ConfigureAwait(false);
            _settings.InboxTableId = await _store.GetTableId(_settings.InboxTable).ConfigureAwait(false);
            _settings.PublishedTableMap = await _store.GetSchema(_settings.PublishedTable).ConfigureAwait(false);
            _settings.ReceivedTableMap = await _store.GetSchema(_settings.ReceivedTable).ConfigureAwait(false);
            _settings.OutboxTableMap = await _store.GetSchema(_settings.OutboxTable).ConfigureAwait(false);
            _settings.InboxTableMap = await _store.GetSchema(_settings.InboxTable).ConfigureAwait(false);
        }
    }
}