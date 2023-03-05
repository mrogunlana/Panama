using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.MySQL.Extensions;
using Panama.Core.Extensions;
using Panama.Core.Security.Interfaces;
using Panama.Core.Security.Resolvers;
using System.Data;
using System.Data.Common;

namespace Panama.Core.CDC.MySQL
{
    internal class Store : IStore
    {
        private readonly IOptions<MySqlOptions> _options;
        private readonly IInitialize _initializer;
        private readonly IStringEncryptor _encryptor;

        public Store(
              IInitialize initializer
            , IOptions<MySqlOptions> options
            , StringEncryptorResolver stringEncryptorResolver)
        {
            _options = options;
            _initializer = initializer;
            _encryptor = stringEncryptorResolver(ResolverKey.Base64); ;
        }

        public async Task Init()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    CREATE TABLE IF NOT EXISTS `{_initializer.Settings.Resolve<MySqlSettings>().PublishedTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `Id` varchar(150) DEFAULT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      `BinaryId` binary(16) NOT NULL,
                      `BinaryName` binary(16) NOT NULL,
                      `BinaryStatus` binary(16)  NOT NULL,
                      `BinaryGroup` binary(16) NOT NULL,
                      `BinaryCorrelationId` binary(16) DEFAULT NULL,
                      `BinaryVersion` binary(16) DEFAULT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_ExpiresAt`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `Id` varchar(150) DEFAULT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      `BinaryId` binary(16) NOT NULL,
                      `BinaryName` binary(16) NOT NULL,
                      `BinaryStatus` binary(16)  NOT NULL,
                      `BinaryGroup` binary(16) NOT NULL,
                      `BinaryCorrelationId` binary(16) DEFAULT NULL,
                      `BinaryVersion` binary(16) DEFAULT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_ExpiresAt`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` (
                      `Key` varchar(128) NOT NULL,
                      `Instance` varchar(256) DEFAULT NULL,
                      `LastLockTime` datetime DEFAULT NULL,
                      PRIMARY KEY (`Key`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    INSERT IGNORE INTO `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@PublishedKey, '', @LastLockTime);
                    
                    INSERT IGNORE INTO `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` (`Key`,`Instance`,`LastLockTime`) 
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
        
        public async Task<Dictionary<int, string>> GetPublishedSchema()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};AllowUserVariables=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SET @TABLE_ID = (SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE `NAME` = @Name limit 1);

                    SELECT TABLE_ID, `NAME`, POS, MTYPE
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS 
                    WHERE TABLE_ID = @TABLE_ID
                    order by POS;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_options.Value.Database}/{_initializer.Settings.Resolve<MySqlSettings>().PublishedTable}",
                });

                var result = new Dictionary<int, string>();
                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (reader.Read())
                    result.Add(reader.GetInt32(0), reader.GetString(1));

                connection.Close();

                return result;
            }
        }

        public async Task<Dictionary<int, string>> GetReceivedSchema()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};AllowUserVariables=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SET @TABLE_ID = (SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE `NAME` = @Name limit 1);

                    SELECT TABLE_ID, `NAME`, POS, MTYPE
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS 
                    WHERE TABLE_ID = @TABLE_ID
                    order by POS;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_options.Value.Database}/{_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable}",
                });

                var result = new Dictionary<int, string>();
                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (reader.Read())
                    result.Add(reader.GetInt32(0), reader.GetString(1));

                connection.Close();

                return result;
            }
        }

        public async Task<int> GetPublishedTableId()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE `NAME` = @Name limit 1;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_options.Value.Database}/{_initializer.Settings.Resolve<MySqlSettings>().PublishedTable}",
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                return result.ToInt();
            }
        }

        public async Task<int> GetReceivedTableId()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE `NAME` = @Name limit 1;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_options.Value.Database}/{_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable}",
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                return result.ToInt();
            }
        }

        public async Task<bool> AcquireLock(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` 
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
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` 
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
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_initializer.Settings.Resolve<MySqlSettings>().LockTable}` 
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

        public async Task ChangeMessageState(string tableName, _Message message, MessageStatus status, object? transaction = null)
        {
            using (var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    UPDATE `{tableName}` 
                    SET `Content`       = @Content,
                        `Retries`       = @Retries,
                        `Expires`       = @Expires,
                        `Status`        = @Status 
                        `BinaryStatus`  = unhex(md5(trim(lower(ifnull(@Status, ''))))) 
                    WHERE `_Id`         = @_Id;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@_Id",
                    DbType = DbType.Int32,
                    Value = message._Id,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Content",
                    DbType = DbType.String,
                    Value = _encryptor.ToString(message.Content),
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Retries",
                    DbType = DbType.Int32,
                    Value = message.Retries,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Expires",
                    DbType = DbType.DateTime,
                    Value = message.Expires,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Status",
                    DbType = DbType.String,
                    Value = message.Status,
                });

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task ChangePublishedState(_Message message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_initializer.Settings.Resolve<MySqlSettings>().PublishedTable, message, status, transaction).ConfigureAwait(false);
        }
        
        public async Task ChangeReceivedState(_Message message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable, message, status, transaction).ConfigureAwait(false);
        }

        public async Task<_Message> StorePublishedMessage(_Message message, object? transaction = null)
        {
            using (var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    INSERT INTO `{_options.Value.Database}`.`{_initializer.Settings.Resolve<MySqlSettings>().PublishedTable}`
                    (`Id`,
                    `CorrelationId`,
                    `Version`,
                    `Name`,
                    `Group`,
                    `Content`,
                    `Retries`,
                    `Created`,
                    `Expires`,
                    `Status`,
                    `BinaryId`,
                    `BinaryName`,
                    `BinaryStatus`,
                    `BinaryGroup`,
                    `BinaryCorrelationId`,
                    `BinaryVersion`)
                    VALUES
                    (@Id,
                     @CorrelationId,
                     @Version,
                     @Name,
                     @Group,
                     @Content,
                     @Retries,
                     NOW(),
                     @Expires,
                     @Status,
                     unhex(md5(trim(lower(ifnull(@Id, ''))))),
                     unhex(md5(trim(lower(ifnull(@Name, ''))))),
                     unhex(md5(trim(lower(ifnull(@Status, ''))))),
                     unhex(md5(trim(lower(ifnull(@Group, ''))))), 
                     unhex(md5(trim(lower(ifnull(@CorrelationId, ''))))), 
                     unhex(md5(trim(lower(ifnull(@Version, ''))))));

                     SELECT LAST_INSERT_ID();"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = message.Id,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@CorrelationId",
                    DbType = DbType.String,
                    Value = message.CorrelationId,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Version",
                    DbType = DbType.String,
                    Value = message.Version,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = message.Name,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Group",
                    DbType = DbType.String,
                    Value = message.Group,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Content",
                    DbType = DbType.String,
                    Value = message.IsContentBase64() 
                        ? message.Content 
                        : _encryptor.ToString(message.Content)
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Retries",
                    DbType = DbType.Int32,
                    Value = message.Retries,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Created",
                    DbType = DbType.DateTime,
                    Value = message.Created,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Expires",
                    DbType = DbType.DateTime,
                    Value = message.Expires,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Status",
                    DbType = DbType.String,
                    Value = message.Status,
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                message._Id = result.ToInt();
                
                return message;
            }
        }

        public async Task<_Message> StoreReceivedMessage(_Message message, object? transaction = null)
        {
            using (var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    INSERT INTO `{_options.Value.Database}`.`{_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable}`
                    (`Id`,
                    `CorrelationId`,
                    `Version`,
                    `Name`,
                    `Group`,
                    `Content`,
                    `Retries`,
                    `Created`,
                    `Expires`,
                    `Status`,
                    `BinaryId`,
                    `BinaryName`,
                    `BinaryStatus`,
                    `BinaryGroup`,
                    `BinaryCorrelationId`,
                    `BinaryVersion`)
                    VALUES
                    (@Id,
                     @CorrelationId,
                     @Version,
                     @Name,
                     @Group,
                     @Content,
                     @Retries,
                     NOW(),
                     @Expires,
                     @Status,
                     unhex(md5(trim(lower(ifnull(@Id, ''))))),
                     unhex(md5(trim(lower(ifnull(@Name, ''))))),
                     unhex(md5(trim(lower(ifnull(@Status, ''))))),
                     unhex(md5(trim(lower(ifnull(@Group, ''))))), 
                     unhex(md5(trim(lower(ifnull(@CorrelationId, ''))))), 
                     unhex(md5(trim(lower(ifnull(@Version, ''))))));

                     SELECT LAST_INSERT_ID();"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = message.Id,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@CorrelationId",
                    DbType = DbType.String,
                    Value = message.CorrelationId,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Version",
                    DbType = DbType.String,
                    Value = message.Version,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = message.Name,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Group",
                    DbType = DbType.String,
                    Value = message.Group,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Content",
                    DbType = DbType.String,
                    Value = message.IsContentBase64() 
                        ? message.Content 
                        : _encryptor.ToString(message.Content)
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Retries",
                    DbType = DbType.Int32,
                    Value = message.Retries,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Created",
                    DbType = DbType.DateTime,
                    Value = message.Created,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Expires",
                    DbType = DbType.DateTime,
                    Value = message.Expires,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Status",
                    DbType = DbType.String,
                    Value = message.Status,
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                message._Id = result.ToInt();
                
                return message;
            }
        }

        public async Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    DELETE FROM `{table}` 
                    WHERE ExpiresAt < @Timeout 
                    AND (StatusName = @Succeeded 
                    OR StatusName = @Failed) 
                    limit @Batch;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Timeout",
                    DbType = DbType.DateTime,
                    Value = timeout,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Succeeded",
                    DbType = DbType.String,
                    Value = MessageStatus.Succeeded.ToString(),
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Failed",
                    DbType = DbType.String,
                    Value = MessageStatus.Failed.ToString(),
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Batch",
                    DbType = DbType.Int32,
                    Value = batch,
                });

                var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();

                return result;
            }
        }

        public async Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_initializer.Settings.Resolve<MySqlSettings>().PublishedTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_initializer.Settings.Resolve<MySqlSettings>().ReceivedTable, timeout, batch, token).ConfigureAwait(false);
        }
    }
}