using Microsoft.Extensions.Options;
using MySqlCdc;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.MySQL.Extensions;
using Panama.Core.Interfaces;
using Panama.Core.Messaging.Interfaces;
using Panama.Core.Security.Interfaces;
using Panama.Core.Security.Resolvers;

namespace Panama.Core.CDC.MySQL.Processors
{
    public class LogTailingProcessor : IProcess
    {
        private readonly BinlogClient _client;
        private readonly Dictionary<int, string> _published;
        private readonly Dictionary<int, string> _received;
        private readonly IStore _store;
        private readonly IOptions<MySqlCdcOptions> _options;
        private readonly IEnumerable<IBroker> _brokers;
        private readonly IStringEncryptor _encryptor;

        public LogTailingProcessor(
              IStore store
            , IOptions<MySqlCdcOptions> options
            , IEnumerable<IBroker> brokers
            , StringEncryptorResolver stringEncryptorResolver)
        {
            //TODO: check the existance of MySqlCdCOptions in the 
            //registrar and if it's null, throw an exception as 
            //its table and database specific values below are required

            _store = store;
            _options = options;
            _brokers = brokers;
            _encryptor = stringEncryptorResolver(ResolverKey.Base64);
            _published = _store.GetSchema(_options.Value.PublishedTableId);
            _received = _store.GetSchema(_options.Value.ReceivedTableId);

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

            _client = new BinlogClient(options =>
            {
                options.Hostname = _options.Value.Host;
                options.Port = _options.Value.Port;
                options.Username = _options.Value.Username;
                options.Password = _options.Value.Password;
                options.SslMode = SslMode.Disabled;
                options.HeartbeatInterval = TimeSpan.FromSeconds(_options.Value.Heartbeat);
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

            // TODO: should we leave the message base64 
            // encrypted and let the consumer decode?
            //.DecodeContent(_encryptor);
            var published = writeRows
                .GetPublishedMessages(_options.Value, _published);

            var received = writeRows
                .GetReceivedMessages(_options.Value, _received);

            
            //publish to message broker
            foreach (var broker in _brokers)
                foreach (var publish in published)
                    await broker.Publish(new MessageContext(publish, context.Provider, context.Token));

            //TODO: received to subscribers
        }
    }
}