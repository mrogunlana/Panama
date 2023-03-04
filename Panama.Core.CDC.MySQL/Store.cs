using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Core.CDC.Interfaces;
using System.Data;

namespace Panama.Core.CDC.MySQL
{
    internal class Store : IStore
    {
        private readonly IOptions<MySqlCdcOptions> _options;

        public Store(
            IOptions<MySqlCdcOptions> options)
        {
            _options = options;
        }

        public Dictionary<int, string> GetSchema(object table)
        {
            if (table == null)
                throw new ArgumentNullException("Table value must be set to retreive schema.");

            int.TryParse(table.ToString(), out var id);

            if (id == 0)
                throw new ArgumentNullException("Table value must be an integer value to retreive schema.");

            using (var connection = new MySqlConnection($"Server={_options.Value.Host};User ID={_options.Value.Username};Password={_options.Value.Password};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    SELECT POS, `NAME`
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS
                    WHERE TABLE_ID = @TABLE_ID
                    AND UPPER(`NAME`) NOT LIKE '%BINARY%'
                    ORDER BY POS;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@TABLE_ID",
                    DbType = DbType.Int32,
                    Value = id,
                });

                var result = new Dictionary<int, string>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    result.Add(reader.GetInt32(0), reader.GetString(1));

                connection.Close();

                return result;
            }
        }

        public async Task InitLocks()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};User ID={_options.Value.Username};Password={_options.Value.Password};Database={_options.Value.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    INSERT IGNORE INTO `Lock` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@PublishedKey, '', @LastLockTime);
                    
                    INSERT IGNORE INTO `Lock` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@ReceivedKey, '', @LastLockTime);"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@PublishedKey",
                    DbType = DbType.String,
                    Value = $"published_retry_{_options.Value.Version}",
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@ReceivedKey",
                    DbType = DbType.String,
                    Value = $"received_retry_{_options.Value.Version}",
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.MinValue,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task<bool> AcquireLock(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};User ID={_options.Value.Username};Password={_options.Value.Password};Database={_options.Value.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET  `Instance`= @Instance
                        ,`LastLockTime`= @LastLockTime 
                    WHERE `Key`= @Key
                    AND `LastLockTime` < @Window;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.Now,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Key",
                    DbType = DbType.String,
                    Value = key,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Window",
                    DbType = DbType.DateTime,
                    Value = DateTime.Now.Subtract(ttl),
                });

                var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();

                return result > 0;
            }
        }

        public async Task ReleaseLock(string key, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};User ID={_options.Value.Username};Password={_options.Value.Password};Database={_options.Value.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET  `Instance` = ''
                        ,`LastLockTime` = @LastLockTime 
                    WHERE `Key` = @Key
                    AND `Instance`= @Instance;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.MinValue,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Key",
                    DbType = DbType.String,
                    Value = key,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};User ID={_options.Value.Username};Password={_options.Value.Password};Database={_options.Value.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET `LastLockTime` = date_add(`LastLockTime`, interval @Ttl second) 
                    WHERE `Key` = @Key
                    AND `Instance` = @Instance;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Ttl",
                    DbType = DbType.Double,
                    Value = ttl.TotalSeconds,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Key",
                    DbType = DbType.String,
                    Value = key,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }
    }
}