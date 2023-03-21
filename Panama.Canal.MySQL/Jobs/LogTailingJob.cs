using Microsoft.Extensions.Options;
using MySqlCdc;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Extensions;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using Quartz;

namespace Panama.Canal.MySQL.Jobs
{
    [DisallowConcurrentExecution]
    public class LogTailingJob : IJob
    {
        private readonly BinlogClient _client;
        private readonly MySqlSettings _settings;
        private readonly IOptions<MySqlOptions> _options;
        private readonly IDispatcher _dispatcher;
        private readonly IStringEncryptor _encryptor;
        private readonly IInitialize _initializer;

        public LogTailingJob(
              MySqlSettings settings
            , IDispatcher dispatcher
            , IInitialize initializer
            , IOptions<MySqlOptions> options
            , StringEncryptorResolver stringEncryptorResolver)
        {
            //TODO: check the existance of MySqlCdCOptions in the 
            //registrar and if it's null, throw an exception as 
            //its table and database specific values below are required

            _settings = settings;
            _options = options;
            _dispatcher = dispatcher;
            _encryptor = stringEncryptorResolver(StringEncryptorResolverKey.Base64);
            _initializer = initializer;

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
                options.Blocking = _options.Value.StreamBinlog;

                // Start replication from MySQL GTID
                //var gtidSet = "4805a37c-b600-11ed-91dc-0242ac1a0002:1-19";
                //options.Binlog = BinlogOptions.FromGtid(GtidSet.Parse(gtidSet));
            });
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await foreach (var binlogEvent in _client.Replicate(context.CancellationToken))
            {
                if (context.CancellationToken.IsCancellationRequested)
                    context.CancellationToken.ThrowIfCancellationRequested();

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

        private async Task HandleWriteRowsEvent(WriteRowsEvent writeRows, IJobExecutionContext context)
        {
            if (context.CancellationToken.IsCancellationRequested)
                context.CancellationToken.ThrowIfCancellationRequested();

            // TODO: should we leave the message base64 
            // encrypted and let the consumer decode?
            var outbox = writeRows
                .GetOutboxMessages(_settings);

            var inbox = writeRows
                .GetInboxMessages(_settings);
            
            //publish to message brokers
            foreach (var publish in outbox)
                await _dispatcher.Publish(
                    message: publish,
                    token: context.CancellationToken);

            //received to subscribers
            foreach (var received in inbox)
                await _dispatcher.Execute(
                    message: received,
                    token: context.CancellationToken);
        }
    }
}