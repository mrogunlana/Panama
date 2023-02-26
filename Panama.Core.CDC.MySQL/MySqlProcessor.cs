using MySqlCdc;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class MySqlProcessor : IProcess
    {
        private readonly BinlogClient _client;

        public MySqlProcessor()
        {
            /*  TODO: get table/column info,
             *  For example: 
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
                options.Port = 3309;
                options.Username = "<REDACTED>";
                options.Password = "<REDACTED>";
                options.SslMode = SslMode.Disabled;
                options.HeartbeatInterval = TimeSpan.FromSeconds(30);
                options.Blocking = true;

                // Start replication from MySQL GTID
                //var gtidSet = "4805a37c-b600-11ed-91dc-0242ac1a0002:1-19";
                //options.Binlog = BinlogOptions.FromGtid(GtidSet.Parse(gtidSet));
            });
        }
        public async Task Invoke(IContext context)
        {
            await foreach (var binlogEvent in _client.Replicate())
            {
                var state = _client.State;

                //TODO: Emit Event 

                //if (binlogEvent is TableMapEvent tableMap)
                //{
                //    await HandleTableMapEvent(tableMap);
                //}
                //else if (binlogEvent is WriteRowsEvent writeRows)
                //{
                //    await HandleWriteRowsEvent(writeRows);
                //}
                //else if (binlogEvent is UpdateRowsEvent updateRows)
                //{
                //    await HandleUpdateRowsEvent(updateRows);
                //}
                //else if (binlogEvent is DeleteRowsEvent deleteRows)
                //{
                //    await HandleDeleteRowsEvent(deleteRows);
                //}
                //else await PrintEventAsync(binlogEvent);
            }
        }
    }
}