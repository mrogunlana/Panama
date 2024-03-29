﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Models.Options;
using Panama.Canal.MySQL.Extensions;
using Panama.Canal.MySQL.Models;
using Panama.Canal.Sagas.Models;
using Panama.Extensions;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Data;
using System.Data.Common;

namespace Panama.Canal.MySQL
{
    internal class Store : IStore
    {
        private readonly ILogger<Store> _log;
        private readonly IOptions<MySqlOptions> _mysqlOptions;
        private readonly IOptions<CanalOptions> _canalOptions;
        private readonly MySqlSettings _settings;
        private readonly IStringEncryptor _encryptor;

        public Store(
              ILogger<Store> log
            , MySqlSettings settings
            , IOptions<MySqlOptions> mysqlOptions
            , IOptions<CanalOptions> canalOptions
            , StringEncryptorResolver stringEncryptorResolver)
        {
            _log = log;
            _settings = settings;
            _mysqlOptions = mysqlOptions;
            _canalOptions = canalOptions;
            _encryptor = stringEncryptorResolver(StringEncryptorResolverKey.Base64);
        }

        public async Task Init()
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                //1. Initialize MySql version information 
                _settings.SetVersion(connection.ServerVersion);

                using var command = new MySqlCommand($@"
                    
                    CREATE TABLE IF NOT EXISTS `{_settings.PublishedTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `__Id` BINARY(16) NOT NULL,
                      `Id` varchar(150) NOT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Broker` varchar(200) DEFAULT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`),
                      INDEX `IX__Id`(`__Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.ReceivedTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `__Id` BINARY(16) NOT NULL,
                      `Id` varchar(150) DEFAULT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Broker` varchar(200) DEFAULT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`),
                      INDEX `IX__Id`(`__Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.OutboxTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `__Id` BINARY(16) NOT NULL,
                      `Id` varchar(150) DEFAULT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Broker` varchar(200) DEFAULT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`),
                      INDEX `IX__Id`(`__Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.InboxTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `__Id` BINARY(16) NOT NULL,
                      `Id` varchar(150) DEFAULT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Version` varchar(20) DEFAULT NULL,
                      `Name` varchar(400) NOT NULL,
                      `Broker` varchar(200) DEFAULT NULL,
                      `Group` varchar(200) DEFAULT NULL,
                      `Content` longtext,
                      `Retries` int(11) DEFAULT NULL,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      `Status` varchar(40) NOT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`),
                      INDEX `IX__Id`(`__Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.SagaTable}` (
                      `_Id` bigint NOT NULL AUTO_INCREMENT,
                      `Id` varchar(150) DEFAULT NULL,
                      `BinaryId` BINARY(16) NOT NULL,
                      `CorrelationId` varchar(150) DEFAULT NULL,
                      `Trigger` varchar(300) DEFAULT NULL,
                      `Source` varchar(300) DEFAULT NULL,
                      `Destination` varchar(300) DEFAULT NULL,
                      `Content` longtext,
                      `Created` datetime NOT NULL,
                      `Expires` datetime DEFAULT NULL,
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`),
                      INDEX `IX_BinaryId`(`BinaryId`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.LockTable}` (
                      `Key` varchar(128) NOT NULL,
                      `Instance` varchar(256) DEFAULT NULL,
                      `LastLockTime` datetime DEFAULT NULL,
                      PRIMARY KEY (`Key`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    INSERT IGNORE INTO `{_settings.LockTable}` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@PublishedKey, '', @LastLockTime);
                    
                    INSERT IGNORE INTO `{_settings.LockTable}` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@ReceivedKey, '', @LastLockTime);"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@PublishedKey",
                    DbType = DbType.String,
                    Value = _canalOptions.Value.GetPublishedRetryKey(),
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@ReceivedKey",
                    DbType = DbType.String,
                    Value = _canalOptions.Value.GetReceivedRetryKey(),
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DBNull.Value,
                });

                //2. Initialize Panama tables
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task<Dictionary<int, string>> GetSchema(string table)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};AllowUserVariables=True;"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SET @TABLE_ID = (SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE replace(`Name`, '@002e', '.') = @Name limit 1);

                    SELECT TABLE_ID, `NAME`, POS, MTYPE
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS 
                    WHERE TABLE_ID = @TABLE_ID
                    order by POS;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_mysqlOptions.Value.Database}/{table}",
                });

                var result = new Dictionary<int, string>();
                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (reader.Read())
                    result.Add(reader.GetInt32(2), reader.GetString(1));

                connection.Close();

                return result;
            }
        }

        public async Task<int> GetPublishedTableId()
        {
            return await GetTableId($@"{_mysqlOptions.Value.Database}/{_settings.PublishedTable}");
        }

        public async Task<int> GetTableId(string table)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand(@"
                    
                    SELECT TABLE_ID  
                    FROM INFORMATION_SCHEMA.INNODB_TABLES 
                    WHERE replace(`Name`, '@002e', '.') = @Name limit 1;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Name",
                    DbType = DbType.String,
                    Value = $@"{_mysqlOptions.Value.Database}/{table}",
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                return result.ToInt();
            }
        }

        public async Task<int> GetReceivedTableId()
        {
            return await GetTableId($@"{_mysqlOptions.Value.Database}/{_settings.ReceivedTable}");
        }

        public async Task<bool> AcquireLock(string key, TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_settings.LockTable}` 
                    SET  `Instance`= @Instance
                        ,`LastLockTime`= @LastLockTime 
                    WHERE `Key`= @Key
                    AND `LastLockTime` < @Window;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance ?? _canalOptions.Value.Instance,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DateTime.UtcNow,
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
                    Value = DateTime.UtcNow.Subtract(ttl),
                });

                var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();

                return result > 0;
            }
        }
        public async Task<bool> AcquirePublishedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            return await AcquireLock(_canalOptions.Value.GetPublishedRetryKey(), ttl, instance, token).ConfigureAwait(false);
        }
        public async Task<bool> AcquireReceivedRetryLock(TimeSpan ttl, string? instance = null, CancellationToken token = default)
        {
            return await AcquireLock(_canalOptions.Value.GetReceivedRetryKey(), ttl, instance, token).ConfigureAwait(false);
        }

        public async Task ReleaseLock(string key, string? instance = null, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_settings.LockTable}` 
                    SET  `Instance` = ''
                        ,`LastLockTime` = @LastLockTime 
                    WHERE `Key` = @Key
                    AND `Instance`= @Instance;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Instance",
                    DbType = DbType.String,
                    Value = instance ?? _canalOptions.Value.Instance,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@LastLockTime",
                    DbType = DbType.DateTime,
                    Value = DBNull.Value,
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
        public async Task ReleasePublishedLock(string? instance = null, CancellationToken token = default)
        {
            await ReleaseLock(_canalOptions.Value.GetPublishedRetryKey(), instance, token).ConfigureAwait(false);
        }
        public async Task ReleaseReceivedLock(string? instance = null, CancellationToken token = default)
        {
            await ReleaseLock(_canalOptions.Value.GetReceivedRetryKey(), instance, token).ConfigureAwait(false);
        }

        public async Task RenewLockAsync(string key, TimeSpan ttl, string instance, CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    UPDATE `{_settings.LockTable}` 
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

        public async Task ChangeMessageState(string tableName, InternalMessage message, MessageStatus status, object? transaction = null)
        {
            var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};Allow User Variables=True;");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                SET @_Id = (SELECT _Id FROM `{tableName}` WHERE `__Id` = unhex(md5(@Id)) LIMIT 1);

                UPDATE `{tableName}` 
                SET `Content`       = @Content,
                    `Retries`       = @Retries,
                    `Expires`       = @Expires,
                    `Status`        = @Status 
                WHERE   `_Id`       = @_Id;";

            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Id",
                DbType = DbType.String,
                Value = message.Id,
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
                ParameterName = "@Expires",
                DbType = DbType.DateTime,
                Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Status",
                DbType = DbType.String,
                Value = status,
            });

            command.Transaction = transaction?.To<DbTransaction>();
            try
            {
                var result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                _log.LogDebug($"{result} message state changed to: {status}; Id: {message.Id}; _Id: {message._Id}");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error occurred during message status update. Message ID: {message.Id}. Message Status: {message.Status}. Updated Status: {status}");
            }
        }

        public async Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_settings.PublishedTable, message, status, transaction).ConfigureAwait(false);
        }

        public async Task ChangeReceivedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_settings.ReceivedTable, message, status, transaction).ConfigureAwait(false);
        }

        public async Task ChangePublishedStateToDelayed(string[] ids)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    UPDATE `{_settings.PublishedTable}` 
                    SET `Status`='{MessageStatus.Delayed}' 
                    WHERE `Id` IN ({string.Join(',', ids)});"

                , connection);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task ChangeReceivedStateToDelayed(string[] ids)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    UPDATE `{_settings.ReceivedTable}` 
                    SET `Status`='{MessageStatus.Delayed}' 
                    WHERE `Id` IN ({string.Join(',', ids)});"

                , connection);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null)
        {
            var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                INSERT INTO `{_mysqlOptions.Value.Database}`.`{_settings.PublishedTable}`
                (`__Id`,
                `Id`,
                `CorrelationId`,
                `Version`,
                `Name`,
                `Broker`,
                `Group`,
                `Content`,
                `Retries`,
                `Created`,
                `Expires`,
                `Status`)
                VALUES
                (unhex(md5(@Id)),
                @Id,
                 @CorrelationId,
                 @Version,
                 @Name,
                 @Broker,
                 @Group,
                 @Content,
                 @Retries,
                 UTC_TIMESTAMP(),
                 @Expires,
                 @Status);
                 
                 SELECT LAST_INSERT_ID();";

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
                Value = message.Version ?? _canalOptions.Value.Version,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Name",
                DbType = DbType.String,
                Value = message.Name,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Broker",
                DbType = DbType.String,
                Value = message.Broker,
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
                Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Status",
                DbType = DbType.String,
                Value = message.Status,
            });

            command.Transaction = transaction?.To<DbTransaction>();

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            message._Id = result.ToInt();

            return message;
        }

        public async Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null)
        {
            var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                INSERT INTO `{_mysqlOptions.Value.Database}`.`{_settings.ReceivedTable}`
                (`__Id`,
                `Id`,
                `CorrelationId`,
                `Version`,
                `Name`,
                `Broker`,
                `Group`,
                `Content`,
                `Retries`,
                `Created`,
                `Expires`,
                `Status`)
                VALUES
                (unhex(md5(@Id)),
                @Id,
                @CorrelationId,
                @Version,
                @Name,
                @Broker,
                @Group,
                @Content,
                @Retries,
                UTC_TIMESTAMP(),
                @Expires,
                @Status);

                SELECT LAST_INSERT_ID();";

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
                Value = message.Version ?? _canalOptions.Value.Version,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Name",
                DbType = DbType.String,
                Value = message.Name,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Broker",
                DbType = DbType.String,
                Value = message.Broker,
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
                Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Status",
                DbType = DbType.String,
                Value = message.Status,
            });

            command.Transaction = transaction?.To<DbTransaction>();

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            message._Id = result.ToInt();

            return message;
        }

        public async Task<InternalMessage> StoreInboxMessage(InternalMessage message, object? transaction = null)
        {
            var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                INSERT INTO `{_mysqlOptions.Value.Database}`.`{_settings.InboxTable}`
                (`__Id`,
                `Id`,
                `CorrelationId`,
                `Version`,
                `Name`,
                `Broker`,
                `Group`,
                `Content`,
                `Retries`,
                `Created`,
                `Expires`,
                `Status`)
                VALUES
                (unhex(md5(@Id)),
                @Id,
                @CorrelationId,
                @Version,
                @Name,
                @Broker,
                @Group,
                @Content,
                @Retries,
                UTC_TIMESTAMP(),
                @Expires,
                @Status);

                SELECT LAST_INSERT_ID();";

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
                Value = message.Version ?? _canalOptions.Value.Version,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Name",
                DbType = DbType.String,
                Value = message.Name,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Broker",
                DbType = DbType.String,
                Value = message.Broker,
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
                Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Status",
                DbType = DbType.String,
                Value = message.Status,
            });

            command.Transaction = transaction?.To<DbTransaction>();

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            message._Id = result.ToInt();

            return message;
        }

        public async Task<InternalMessage> StoreOutboxMessage(InternalMessage message, object? transaction = null)
        {
            var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                INSERT INTO `{_mysqlOptions.Value.Database}`.`{_settings.OutboxTable}`
                (`__Id`,
                `Id`,
                `CorrelationId`,
                `Version`,
                `Name`,
                `Broker`,
                `Group`,
                `Content`,
                `Retries`,
                `Created`,
                `Expires`,
                `Status`)
                VALUES
                (unhex(md5(@Id)),
                @Id,
                @CorrelationId,
                @Version,
                @Name,
                @Broker,
                @Group,
                @Content,
                @Retries,
                UTC_TIMESTAMP(),
                @Expires,
                @Status);

                SELECT LAST_INSERT_ID();";

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
                Value = message.Version ?? _canalOptions.Value.Version,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Name",
                DbType = DbType.String,
                Value = message.Name,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Broker",
                DbType = DbType.String,
                Value = message.Broker,
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
                Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Status",
                DbType = DbType.String,
                Value = message.Status,
            });

            command.Transaction = transaction?.To<DbTransaction>();

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            message._Id = result.ToInt();

            return message;
        }

        public async Task<int> DeleteExpiredAsync(string table, DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            _log.LogDebug($"Deleting expired data from table: {table}.");

            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    DELETE FROM `{table}` 
                    WHERE Expires < @Timeout 
                    AND (Status = @Succeeded 
                    OR Status = @Failed) 
                    limit @Batch;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Timeout",
                    DbType = DbType.DateTime,
                    Value = timeout,
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Succeeded",
                    DbType = DbType.String,
                    Value = MessageStatus.Succeeded.ToString(),
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Failed",
                    DbType = DbType.String,
                    Value = MessageStatus.Failed.ToString(),
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Batch",
                    DbType = DbType.Int32,
                    Value = batch,
                });

                var result = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                connection.Close();

                _log.LogDebug($"Deleted {result} expired message(s) from table: {table} complete.");

                return result;
            }

            
        }

        public async Task<int> DeleteExpiredPublishedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_settings.PublishedTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_settings.ReceivedTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<int> DeleteExpiredInboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_settings.InboxTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<int> DeleteExpiredOutboxAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_settings.OutboxTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    SELECT 
                         `_Id`
                        ,`Id` 
                        ,`CorrelationId`
                        ,`Version`
                        ,`Name` 
                        ,`Group` 
                        ,`Content` 
                        ,`Retries` 
                        ,`Created` 
                        ,`Expires` 
                        ,`Status` 
                    FROM `{table}` 
                    WHERE `Retries` < @Retries
                    AND `Version` = @Version 
                    AND `Created` < @Created 
                    AND (`Status` = '{MessageStatus.Failed}' OR `Status` = '{MessageStatus.Scheduled}') 
                    LIMIT 200;"

                , connection);

                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Retries",
                    DbType = DbType.Int32,
                    Value = _mysqlOptions.Value.FailedRetries
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Version",
                    DbType = DbType.String,
                    Value = _canalOptions.Value.Version
                });
                command.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@Created",
                    DbType = DbType.String,
                    Value = DateTime.UtcNow.AddMinutes(-4)
                });

                var messages = new List<InternalMessage>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = _settings.GetModel(table);
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<InternalMessage>(reader.GetName(i), reader.GetValue(i));

                    messages.Add(model);
                }

                connection.Close();

                return messages;
            }
        }

        public async Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry()
        {
            return await GetMessagesToRetry(_settings.PublishedTable).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry()
        {
            return await GetMessagesToRetry(_settings.ReceivedTable).ConfigureAwait(false);
        }

        public async Task GetDelayedMessagesForScheduling(
              string table
            , Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};Allow User Variables=True;"))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, token))
                {
                    var append = _settings
                        .Resolve<MySqlSettings>()
                        .IsSupportSkipLocked() ? "FOR UPDATE SKIP LOCKED" : "FOR UPDATE";

                    var messages = new List<InternalMessage>();

                    using (var command = new MySqlCommand($@"
                    
                    SELECT 
                         `_Id` 
                        ,`Id`    
                        ,`CorrelationId`
                        ,`Version`
                        ,`Name` 
                        ,`Group` 
                        ,`Content` 
                        ,`Retries` 
                        ,`Created` 
                        ,`Expires` 
                        ,`Status` 
                    FROM `{table}` 
                    WHERE `Retries` < @Retries 
                    AND `Version` = @Version 
                    AND ((`Expires`< @TwoMinutesLater AND `Status` = '{MessageStatus.Delayed}') 
                        OR (`Expires`< @OneMinutesAgo AND `Status` = '{MessageStatus.Queued}')) 
                    {append};"

                    , connection))
                    {
                        command.Parameters.Add(new MySqlParameter
                        {
                            ParameterName = "@Version",
                            DbType = DbType.String,
                            Value = _canalOptions.Value.Version
                        });
                        command.Parameters.Add(new MySqlParameter
                        {
                            ParameterName = "@Retries",
                            DbType = DbType.Int32,
                            Value = _canalOptions.Value.FailedRetryCount
                        });
                        command.Parameters.Add(new MySqlParameter
                        {
                            ParameterName = "@TwoMinutesLater",
                            DbType = DbType.String,
                            Value = DateTime.UtcNow.AddMinutes(2)
                        });
                        command.Parameters.Add(new MySqlParameter
                        {
                            ParameterName = "@OneMinutesAgo",
                            DbType = DbType.String,
                            Value = DateTime.UtcNow.AddMinutes(-1)
                        });
                        command.Transaction = transaction;

                        using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync().ConfigureAwait(false))
                            {
                                var model = _settings.GetModel(table);

                                for (int i = 0; i < reader.FieldCount; i++)
                                    model.SetValue<InternalMessage>(reader.GetName(i), reader.GetValue(i));

                                messages.Add(model);
                            }
                        }
                    }

                    _log.LogDebug($"Retrieved {messages.Count} delayed message(s) for scheduling.");

                    await task(transaction, messages);

                    await transaction.CommitAsync(token);
                }
            }
        }

        public async Task GetDelayedPublishedMessagesForScheduling(
              Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            await GetDelayedMessagesForScheduling(
                _settings.PublishedTable
                , task
                , token)
                .ConfigureAwait(false);
        }

        public async Task GetDelayedReceivedMessagesForScheduling(
              Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            await GetDelayedMessagesForScheduling(
                _settings.ReceivedTable
                , task
                , token)
                .ConfigureAwait(false);
        }

        public async Task<SagaEvent> StoreSagaEvent(SagaEvent saga)
        {
            var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};");
            if (connection.State == ConnectionState.Closed)
                await connection.OpenAsync().ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = $@"

                INSERT INTO `{_mysqlOptions.Value.Database}`.`{_settings.SagaTable}`
                (`Id`,
                `BinaryId`,
                `CorrelationId`,
                `Content`,
                `Trigger`,
                `Source`,
                `Destination`,
                `Created`,
                `Expires`)
                VALUES
                (@Id,
                UNHEX(MD5(TRIM(LOWER(@Id)))),
                @CorrelationId,
                @Content,
                @Trigger,
                @Source,
                @Destination,
                UTC_TIMESTAMP(),
                @Expires);

                SELECT LAST_INSERT_ID();";

            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Id",
                DbType = DbType.String,
                Value = saga.Id,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@CorrelationId",
                DbType = DbType.String,
                Value = saga.CorrelationId,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Content",
                DbType = DbType.String,
                Value = saga.Content.IsContentBase64()
                    ? saga.Content
                    : _encryptor.ToString(saga.Content ?? string.Empty)
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Trigger",
                DbType = DbType.String,
                Value = saga.Trigger,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Source",
                DbType = DbType.String,
                Value = saga.Source,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Destination",
                DbType = DbType.String,
                Value = saga.Destination,
            });
            
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Created",
                DbType = DbType.DateTime,
                Value = saga.Created,
            });
            command.Parameters.Add(new MySqlParameter {
                ParameterName = "@Expires",
                DbType = DbType.DateTime,
                Value = saga.Expires.HasValue ? saga.Expires.Value : DBNull.Value,
            });

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            connection.Close();

            saga._Id = result.ToInt();

            return saga;
        }

        public async Task<IEnumerable<SagaEvent>> GetSagaEvents(string id)
        {
            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    
                    SELECT 
                         `_Id`
                        ,`Id` 
                        ,`CorrelationId`
                        ,`Content`
                        ,`Trigger` 
                        ,`Source` 
                        ,`Destination` 
                        ,`Created` 
                        ,`Expires`
                    FROM `{_mysqlOptions.Value.Database}`.`{_settings.SagaTable}`
                    WHERE `BinaryId` = UNHEX(MD5(TRIM(LOWER(@Id))))
                    LIMIT 200;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = id
                });

                var results = new List<SagaEvent>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = new SagaEvent();
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<SagaEvent>(reader.GetName(i), reader.GetValue(i));

                    results.Add(model);
                }

                connection.Close();

                return results;
            }
        }

        public async Task<int> DeleteExpiredSagaEvents(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            
            _log.LogDebug($"Deleting expired data from table: {_settings.SagaTable}.");

            using (var connection = new MySqlConnection($"Server={_mysqlOptions.Value.Host};Port={_mysqlOptions.Value.Port};Database={_mysqlOptions.Value.Database};Uid={_mysqlOptions.Value.Username};Pwd={_mysqlOptions.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"
                    DELETE FROM `{_mysqlOptions.Value.Database}`.`{_settings.SagaTable}`
                    WHERE Expires < @Timeout 
                    limit @Batch;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Timeout",
                    DbType = DbType.DateTime,
                    Value = timeout,
                });
                
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Batch",
                    DbType = DbType.Int32,
                    Value = batch,
                });

                var result = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                connection.Close();

                _log.LogDebug($"Deleting {result} message(s) from table: {_settings.SagaTable} complete.");

                return result;
            }


        }
    }
}