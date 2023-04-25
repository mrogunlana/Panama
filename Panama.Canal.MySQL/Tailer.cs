using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlCdc;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Extensions;
using Panama.Models;
using Panama.Security.Resolvers;

namespace Panama.Canal
{
    public class Tailer : IHostedService, ITailer
    {
        private bool _off;
        private CancellationTokenSource? _cts;

        private readonly BinlogClient _client;
        private readonly MySqlSettings _settings;
        private readonly ILogger<Tailer> _log;
        private readonly IProcessorFactory _factory;
        private readonly IOptions<MySqlOptions> _options;
        private readonly IServiceProvider _provider;
        
        public bool Online => !_cts?.IsCancellationRequested ?? false;

        public Tailer(
              ILogger<Tailer> log
            , IProcessorFactory factory
            , IOptions<MySqlOptions> options
            , IOptions<MySqlSettings> settings
            , IServiceProvider serviceProvider
            , StringEncryptorResolver stringEncryptorResolver)
        {
            //TODO: check the existance of MySqlCdCOptions in the 
            //registrar and if it's null, throw an exception as 
            //its table and database specific values below are required
            _log = log;
            _factory = factory;
            _options = options;
            _settings = settings.Value;
            _provider = serviceProvider;

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

        private async Task Listen()
        {
            await foreach (var binlogEvent in _client.Replicate(_cts!.Token))
            {
                if (_cts.Token.IsCancellationRequested)
                    _cts.Token.ThrowIfCancellationRequested();

                //TODO: Handle Other Events ? e.g: 
                //if tableMap
                //if WriteRowsEvent 
                //if UpdateRowsEvent 
                //if DeleteRowsEvent 
                //if PrintEventAsync 

                if (binlogEvent is WriteRowsEvent writeRows)
                    await Handle(writeRows);
            }
        }

        private async Task Handle(WriteRowsEvent writeRows)
        {
            if (_cts!.Token.IsCancellationRequested)
                _cts!.Token.ThrowIfCancellationRequested();

            // TODO: should we leave the message base64 
            // encrypted and let the consumer decode?
            var outbox = writeRows
                .GetOutboxMessages(_settings);

            var inbox = writeRows
                .GetInboxMessages(_settings);

            var data = outbox.GetData<Message>(_provider);

            //received to subscribers
            foreach (var received in inbox)
                await _factory
                    .GetConsumerProcessor(received)
                    .Execute(new Context()
                        .Add(received)
                        .Token(_cts.Token));

            //publish to message brokers
            foreach (var publish in outbox)
            {
                var message = data.Where(x => x.Headers[Headers.Id] == publish.Id).FirstOrDefault();
                if (message == null)
                    throw new InvalidOperationException("Message headers cannot be found.");

                await _factory
                    .GetProducerProcessor(publish)
                    .Execute(new Context()
                        .Add(message)
                        .Token(_cts.Token));
            }
        }

        public Task Off(CancellationToken cancellationToken)
        {
            if (_off)
                return Task.CompletedTask;

            _cts?.Cancel();

            _cts?.Dispose();
            _cts = null;
            _off = true;

            return Task.CompletedTask;
        }

        public Task On(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_cts != null)
            {
                _log.LogInformation("### Panama Canal MySql Stream Service is already started!");

                return Task.CompletedTask;
            }

            _log.LogDebug("### Panama Canal MySql Stream Service is starting.");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _ = Task.Factory.StartNew(async () => await Listen(), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);

            _off = false;
            _log.LogInformation("### Panama Canal MySql Stream Service started!");

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await On(cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Off(cancellationToken).ConfigureAwait(false);
        }
    }
}
