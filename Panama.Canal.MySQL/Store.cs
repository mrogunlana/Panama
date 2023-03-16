﻿using Microsoft.Extensions.Options;
using MySqlConnector;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Extensions;
using Panama.Extensions;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Data;

namespace Panama.Canal.MySQL
{
    internal class Store : IStore
    {
        private readonly IOptions<MySqlOptions> _options;
        private readonly IOptions<MySqlSettings> _settings;
        private readonly IStringEncryptor _encryptor;

        public Store(
              IOptions<MySqlSettings> settings
            , IOptions<MySqlOptions> options
            , StringEncryptorResolver stringEncryptorResolver)
        {
            _options = options;
            _settings = settings;
            _encryptor = stringEncryptorResolver(StringEncryptorResolverKey.Base64); ;
        }

        public async Task Init()
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                //1. Initialize MySql version information 
                _settings.Value.SetVersion(connection.ServerVersion);

                using var command = new MySqlCommand($@"
                    
                    CREATE TABLE IF NOT EXISTS `{_settings.Value.PublishedTable}` (
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
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.Value.ReceivedTable}` (
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
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.Value.OutboxTable}` (
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
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.Value.InboxTable}` (
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
                      PRIMARY KEY (`_Id`),
                      INDEX `IX_Expires`(`Expires`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS `{_settings.Value.LockTable}` (
                      `Key` varchar(128) NOT NULL,
                      `Instance` varchar(256) DEFAULT NULL,
                      `LastLockTime` datetime DEFAULT NULL,
                      PRIMARY KEY (`Key`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    INSERT IGNORE INTO `{_settings.Value.LockTable}` (`Key`,`Instance`,`LastLockTime`) 
                    VALUES (@PublishedKey, '', @LastLockTime);
                    
                    INSERT IGNORE INTO `{_settings.Value.LockTable}` (`Key`,`Instance`,`LastLockTime`) 
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

                //2. Initialize Panama tables
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task<Dictionary<int, string>> GetSchema(string table)
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
                    Value = $@"{_options.Value.Database}/{table}",
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
                    Value = $@"{_options.Value.Database}/{_settings.Value.PublishedTable}",
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                return result.ToInt();
            }
        }

        public async Task<int> GetTableId(string table)
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
                    Value = $@"{_options.Value.Database}/{table}",
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
                    Value = $@"{_options.Value.Database}/{_settings.Value.ReceivedTable}",
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
                    
                    UPDATE `{_settings.Value.LockTable}` 
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
                    
                    UPDATE `{_settings.Value.LockTable}` 
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
                    
                    UPDATE `{_settings.Value.LockTable}` 
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
                    Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
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

        public async Task ChangePublishedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_settings.Value.PublishedTable, message, status, transaction).ConfigureAwait(false);
        }
        
        public async Task ChangeReceivedState(InternalMessage message, MessageStatus status, object? transaction = null)
        {
            await ChangeMessageState(_settings.Value.ReceivedTable, message, status, transaction).ConfigureAwait(false);
        }

        public async Task ChangePublishedStateToDelayed(int[] ids)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    UPDATE `{_settings.Value.PublishedTable}` 
                    SET `Status`='{MessageStatus.Delayed}' 
                    WHERE `_Id` IN ({string.Join(',', ids)});"

                , connection);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task ChangeReceivedStateToDelayed(int[] ids)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    UPDATE `{_settings.Value.ReceivedTable}` 
                    SET `Status`='{MessageStatus.Delayed}' 
                    WHERE `_Id` IN ({string.Join(',', ids)});"

                , connection);

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                connection.Close();
            }
        }

        public async Task<InternalMessage> StorePublishedMessage(InternalMessage message, object? transaction = null)
        {
            using (var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    INSERT INTO `{_options.Value.Database}`.`{_settings.Value.PublishedTable}`
                    (`Id`,
                    `CorrelationId`,
                    `Version`,
                    `Name`,
                    `Group`,
                    `Content`,
                    `Retries`,
                    `Created`,
                    `Expires`,
                    `Status`)
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
                     @Status);

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
                    Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
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

        public async Task<InternalMessage> StoreReceivedMessage(InternalMessage message, object? transaction = null)
        {
            using (var connection = transaction?.GetConnection() ?? new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed) 
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new MySqlCommand($@"

                    INSERT INTO `{_options.Value.Database}`.`{_settings.Value.ReceivedTable}`
                    (`Id`,
                    `CorrelationId`,
                    `Version`,
                    `Name`,
                    `Group`,
                    `Content`,
                    `Retries`,
                    `Created`,
                    `Expires`,
                    `Status`)
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
                     @Status);

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
                    Value = message.Expires.HasValue ? message.Expires.Value : DBNull.Value,
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
                    WHERE Expires < @Timeout 
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
            return await DeleteExpiredAsync(_settings.Value.PublishedTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<int> DeleteExpiredReceivedAsync(DateTime timeout, int batch = 1000, CancellationToken token = default)
        {
            return await DeleteExpiredAsync(_settings.Value.ReceivedTable, timeout, batch, token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InternalMessage>> GetMessagesToRetry(string table)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
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
                    AND `Added` < @Added 
                    AND (`Status` = '{MessageStatus.Failed}' OR `Status` = '{MessageStatus.Scheduled}') 
                    LIMIT 200;"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Retries",
                    DbType = DbType.Int32,
                    Value = _options.Value.FailedRetries
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Version",
                    DbType = DbType.String,
                    Value = _options.Value.Version
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Added",
                    DbType = DbType.String,
                    Value = DateTime.Now.AddMinutes(-4)
                });

                var messages = new List<InternalMessage>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                var map = _settings.Value.GetMap(table);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = _settings.Value.GetModel(table);
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<InternalMessage>(map[i], reader.GetValue(i));

                    messages.Add(model);
                }
                
                connection.Close();

                return messages;
            }
        }

        public async Task<IEnumerable<InternalMessage>> GetPublishedMessagesToRetry()
        {
            return await GetMessagesToRetry(_settings.Value.PublishedTable).ConfigureAwait(false);
        }

        public async Task<IEnumerable<InternalMessage>> GetReceivedMessagesToRetry()
        {
            return await GetMessagesToRetry(_settings.Value.PublishedTable).ConfigureAwait(false);
        }

        public async Task GetDelayedMessagesForScheduling(
              string table
            , Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            using (var connection = new MySqlConnection($"Server={_options.Value.Host};Port={_options.Value.Port};Database={_options.Value.Database};Uid={_options.Value.Username};Pwd={_options.Value.Password};"))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                var append = _settings.Value
                    .Resolve<MySqlSettings>()
                    .IsSupportSkipLocked() ? "FOR UPDATE SKIP LOCKED" : "FOR UPDATE";

                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
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
                    AND ((`Expires`< @TwoMinutesLater AND `StatusName` = '{MessageStatus.Delayed}') 
                        OR (`Expires`< @OneMinutesAgo AND `StatusName` = '{MessageStatus.Queued}'))
                    LIMIT 200 {append};"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Version",
                    DbType = DbType.String,
                    Value = _options.Value.Version
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@TwoMinutesLater",
                    DbType = DbType.String,
                    Value = DateTime.Now.AddMinutes(2)
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@OneMinutesAgo",
                    DbType = DbType.String,
                    Value = DateTime.Now.AddMinutes(-1)
                });

                var messages = new List<InternalMessage>();
                var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                var map = _settings.Value.GetMap(table);

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var model = _settings.Value.GetModel(table);
                    for (int i = 0; i < reader.FieldCount; i++)
                        model.SetValue<InternalMessage>(map[i], reader.GetValue(i));

                    messages.Add(model);
                }

                await task(transaction, messages);

                await transaction.CommitAsync(token);
            }
        }

        public async Task GetDelayedPublishedMessagesForScheduling(
              string table
            , Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            await GetDelayedMessagesForScheduling(
                _settings.Value.PublishedTable
                , task
                , token)
                .ConfigureAwait(false);
        }

        public async Task GetDelayedReceivedMessagesForScheduling(
              string table
            , Func<object, IEnumerable<InternalMessage>, Task> task
            , CancellationToken token = default)
        {
            await GetDelayedMessagesForScheduling(
                _settings.Value.ReceivedTable
                , task
                , token)
                .ConfigureAwait(false);
        }
    }
}