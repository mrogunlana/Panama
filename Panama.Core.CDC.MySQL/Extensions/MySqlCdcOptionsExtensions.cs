using MySqlConnector;
using System.Data;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class MySqlCdcOptionsExtensions
    {
        internal static Dictionary<int, string> GetSchema(this MySqlCdcOptions settings)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    SELECT POS, `NAME`
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS
                    WHERE TABLE_ID = @TABLE_ID
                    AND UPPER(`NAME`) NOT LIKE '%BINARY%'
                    ORDER BY POS;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@TABLE_ID",
                    DbType = DbType.Int32,
                    Value = settings.OutboxTableId,
                });

                var result = new Dictionary<int, string>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    result.Add(reader.GetInt32(0), reader.GetString(1));

                connection.Close();

                return result;
            }
        }

        internal static async Task InitLocks(this MySqlCdcOptions settings)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};Database={settings.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    INSERT IGNORE INTO `Lock` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@PublishedKey, '', @LastLockTime);
                    
                    INSERT IGNORE INTO `Lock` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@ReceivedKey, '', @LastLockTime);"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@PublishedKey",
                    DbType = DbType.String,
                    Value = $"published_retry_{settings.Version}",
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@ReceivedKey",
                    DbType = DbType.String,
                    Value = $"received_retry_{settings.Version}",
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.MinValue,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                
                connection.Close();
            }
        }
        
        internal static async Task<bool> AcquireLock(this MySqlCdcOptions settings, string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};Database={settings.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET  `Instance`= @Instance
                        ,`LastLockTime`= @LastLockTime 
                    WHERE `Key`= @Key
                    AND `LastLockTime` < @Window;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.Now,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Key",
                    DbType = DbType.String,
                    Value = key,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Window",
                    DbType = DbType.DateTime,
                    Value = DateTime.Now.Subtract(ttl),
                });

                var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                
                connection.Close();

                return result > 0;
            }
        }

        internal static async Task ReleaseLock(this MySqlCdcOptions settings, string key, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};Database={settings.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET  `Instance` = ''
                        ,`LastLockTime` = @LastLockTime 
                    WHERE `Key` = @Key
                    AND `Instance`= @Instance;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.MinValue,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Key",
                    DbType = DbType.String,
                    Value = key,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                
                connection.Close();
            }
        }

        internal static async Task RenewLockAsync(this MySqlCdcOptions settings, string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};Database={settings.Database};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    UPDATE `Lock` 
                    SET `LastLockTime` = date_add(`LastLockTime`, interval @Ttl second) 
                    WHERE `Key` = @Key
                    AND `Instance` = @Instance;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Ttl",
                    DbType = DbType.Double,
                    Value = ttl.TotalSeconds,
                });
                command.Parameters.Add(new MySqlParameter {
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
