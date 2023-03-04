using Microsoft.Extensions.DependencyInjection;
using MySqlCdc;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.MySQL.Extensions;
using Panama.Core.Interfaces;
using Panama.Core.Messaging.Interfaces;
using Panama.Core.Security;
using Panama.Core.Security.Interfaces;
using Panama.Core.Security.Resolvers;

namespace Panama.Core.CDC.MySQL.Processors
{
    public class LogTailingProcessor : IProcess
    {
        private readonly BinlogClient _client;
        private readonly Dictionary<int, string> _map;
        private readonly MySqlCdcOptions _settings;
        private readonly IEnumerable<IBroker> _brokers;
        private readonly IStringEncryptor _encryptor;

        public LogTailingProcessor(MySqlCdcOptions settings, IEnumerable<IBroker> brokers, StringEncryptorResolver stringEncryptorResolver)
        {
            _settings = settings!;
            _brokers = brokers;
            _encryptor = stringEncryptorResolver(ResolverKey.Base64);

            /*  NOTES: 
             * 
             *  #Use for MySQL GTID below
             *  select @@gtid_executed;
             *
             *  SET @TABLE_ID = 1067;
             *
             *  SELECT TABLE_ID, `NAME` 
             *  FROM INFORMATION_SCHEMA.INNODB_TABLES 
             *  WHERE TABLE_ID = @TABLE_ID;
             *
             *  SELECT TABLE_ID, `NAME`, POS, MTYPE
             *  FROM INFORMATION_SCHEMA.INNODB_COLUMNS 
             *  WHERE TABLE_ID = @TABLE_ID
             *  order by POS;
             */

            _map = _settings.GetSchema();
            _client = new BinlogClient(options =>
            {
                options.Hostname = _settings.Host;
                options.Port = _settings.Port;
                options.Username = _settings.Username;
                options.Password = _settings.Password;
                options.SslMode = SslMode.Disabled;
                options.HeartbeatInterval = TimeSpan.FromSeconds(_settings.Heartbeat);
                options.Blocking = true;

                // Start replication from MySQL GTID
                //var gtidSet = "4805a37c-b600-11ed-91dc-0242ac1a0002:1-19";
                //options.Binlog = BinlogOptions.FromGtid(GtidSet.Parse(gtidSet));
            });
        }

        public async Task Invoke(IContext context)
        {
            await foreach (var binlogEvent in _client.Replicate(context.Token))
            {
                if (context.Token.IsCancellationRequested)
                    context.Token.ThrowIfCancellationRequested();

                //TODO: Handle Other Events ? e.g: 
                //if tableMap
                //if WriteRowsEvent 
                //if UpdateRowsEvent 
                //if DeleteRowsEvent 
                //if PrintEventAsync 

                if (binlogEvent is WriteRowsEvent writeRows)
                    await HandleWriteRowsEvent(writeRows, context);
            }
        }

        private async Task HandleWriteRowsEvent(WriteRowsEvent writeRows, IContext context)
        {
            if (context.Token.IsCancellationRequested)
                context.Token.ThrowIfCancellationRequested();

            var messages = writeRows
                .GetMessages(_settings, _map)
                .DecodeContent(_encryptor);
            
            foreach (var broker in _brokers)
                foreach (var message in messages)
                    await broker.Publish(new OutboxContext(message, context.Provider, context.Token));
        }
    }
}