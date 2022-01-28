using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Newtonsoft.Json;
using Panama.Core.Entities;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MySqlData = MySqlConnector;
using MySqlDataBatch = MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Panama.Core.Sql;

namespace Panama.Core.MySql.Dapper
{
    public class MySqlQueryAsync : IQueryAsync
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public MySqlQueryAsync(ILog log)
        {
            _log = log;
            _sql = new SqlGeneratorImpl(new DapperExtensions.DapperExtensionsConfiguration());
            _connection = ConfigurationManager.AppSettings["Database"];

            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};";
        }

        public async Task<List<T>> GetAsync<T>(string sql, object parameters)
        {
            var result = new List<T>();

            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await connection.OpenAsync();

                var query = await connection.QueryAsync<T>(sql, parameters);

                result = query.ToList();

                await connection.CloseAsync();
            }

            return result.ToList();
        }

        public async Task<List<T>> GetAsync<T>(string connection, string sql, object parameters)
        {
            var result = new List<T>();
            
            //var test = new MySql.Data.MySqlClient()
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();

                var query = await c.QueryAsync<T>(sql, parameters);

                result = query.ToList();

                await c.CloseAsync();
            }

            return result.ToList();
        }

        public async Task<T> GetSingleAsync<T>(string sql, object parameters)
        {
            var result = await GetAsync<T>(sql, parameters);

            return result.FirstOrDefault();
        }

        public async Task<T> GetSingleAsync<T>(string connection, string sql, object parameters)
        {
            var result = await GetAsync<T>(connection, sql, parameters);

            return result.FirstOrDefault();
        }

        public async Task InsertAsync<T>(T obj) where T : class
        {
            await Task.Run(() => {
                using (var connection = new MySqlData.MySqlConnection(_connection))
                {
                    connection.Open();

                    connection.Insert(obj);

                    connection.Close();
                }
            });
        }

        public async Task InsertAsync<T>(string connection, T obj) where T : class
        {
            await Task.Run(() => {
                using (var c = new MySqlData.MySqlConnection(connection))
                {
                    c.Open();

                    c.Insert(obj);

                    c.Close();
                }
            });
        }

        public async Task UpdateAsync<T>(T obj) where T : class
        {
            await Task.Run(() => {
                using (var connection = new MySqlData.MySqlConnection(_connection))
                {
                    connection.Open();
                    connection.Update(obj);
                    connection.Close();
                }
            });
        }

        public async Task UpdateAsync<T>(string connection, T obj) where T : class
        {
            await Task.Run(() => {
                using (var c = new MySqlData.MySqlConnection(connection))
                {
                    c.Open();
                    c.Update(obj);
                    c.Close();
                }
            });
        }

        public async Task SaveAsync<T>(T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = await GetAsync<T>($"select * from `{ _sql.Configuration.GetMap<T>().TableName }` where {properties}", parameters);
            if (exist.Count == 0)
                await InsertAsync(obj);
            else
                await UpdateAsync(obj);
        }

        public async Task SaveAsync<T>(string connection, T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = await GetAsync<T>(connection, $"select * from `{ _sql.Configuration.GetMap<T>().TableName }` where {properties}", parameters);
            if (exist.Count == 0)
                await InsertAsync(connection, obj);
            else
                await UpdateAsync(connection, obj);
        }

        public async Task<bool> ExistAsync<T>(string sql, object parameters) where T : class, IModel
        {
            var exist = await GetAsync<T>(sql, parameters);
            if (exist.Count == 0)
                return false;

            return true;
        }

        public async Task<bool> ExistAsync<T>(string connection, string sql, object parameters) where T : class, IModel
        {
            var exist = await GetAsync<T>(connection, sql, parameters);
            if (exist.Count == 0)
                return false;

            return true;
        }

        public async Task DeleteAsync<T>(T obj) where T : class, IModel
        {
            await Task.Run(() => {
                using (var connection = new MySqlData.MySqlConnection(_connection))
                {
                    connection.Open();
                    connection.Delete(obj);
                    connection.Close();
                }
            });
        }

        public async Task DeleteAsync<T>(string connection, T obj) where T : class, IModel
        {
            await Task.Run(() => {
                using (var c = new MySqlData.MySqlConnection(connection))
                {
                    c.Open();
                    c.Delete(obj);
                    c.Close();
                }
            });
        }

        public async Task ExecuteAsync(string sql, object parameters)
        {
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, parameters);
                await connection.CloseAsync();
            }
        }

        public async Task ExecuteAsync(string connection, string sql, object parameters)
        {
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();
                await c.ExecuteAsync(sql, parameters);
                await c.CloseAsync();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters)
        {
            T result = default;

            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await connection.OpenAsync();

                result = await connection.ExecuteScalarAsync<T>(sql, parameters);

                await connection.CloseAsync();
            }

            return result;
        }

        public async Task<T> ExecuteScalarAsync<T>(string connection, string sql, object parameters)
        {
            T result = default;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();

                result = await c.ExecuteScalarAsync<T>(sql, parameters);

                await c.CloseAsync();
            }

            return result;
        }

        public async Task InsertBatchAsync<T>(List<T> models, int batch = 0) where T : class, IModel
        {
            await Task.Run(() => {

                var mysql = new MySqlData.MySqlConnectionStringBuilder(_connection);
                var database = mysql.Database;

                var map = _sql?.Configuration?.GetMap<T>();
                if (map == null)
                    throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");

                var name = map.TableName;
                var table = models.ToDataTable();
                if (table.Rows.Count == 0)
                    return;

                var builder = new StringBuilder();
                builder.Append("SELECT TABLE_NAME");
                builder.Append(", COLUMN_NAME");
                builder.Append(", DATA_TYPE");
                builder.Append(", CHARACTER_MAXIMUM_LENGTH");
                builder.Append(", CHARACTER_OCTET_LENGTH");
                builder.Append(", NUMERIC_PRECISION");
                builder.Append(", NUMERIC_SCALE AS SCALE");
                builder.Append(", COLUMN_DEFAULT");
                builder.Append(", IS_NULLABLE");
                builder.Append(" FROM INFORMATION_SCHEMA.COLUMNS");
                builder.Append(" WHERE TABLE_NAME = @Table");
                builder.Append(" AND TABLE_SCHEMA = @Database");

                var schema = new List<Schema>();

                //get table schema (e.g. names and datatypes for mapping)
                using (var c = new MySqlData.MySqlConnection(_connection))
                using (var command = new MySqlData.MySqlCommand(builder.ToString(), c))
                {
                    var parameter = new MySqlData.MySqlParameter();
                    parameter.Value = map.TableName;
                    parameter.ParameterName = "@Table";
                    parameter.MySqlDbType = MySqlData.MySqlDbType.String;

                    var parameter2 = new MySqlData.MySqlParameter();
                    parameter2.Value = database;
                    parameter2.ParameterName = "@Database";
                    parameter2.MySqlDbType = MySqlData.MySqlDbType.String;

                    command.Parameters.Add(parameter);
                    command.Parameters.Add(parameter2);

                    using (var sql = new MySqlData.MySqlDataAdapter(command))
                    {
                        var result = new DataTable();
                        var parameters = map.Properties
                            .Where(x => x.Ignored == false)
                            .Where(x => x.IsReadOnly == false)
                            .Where(x => x.KeyType == KeyType.NotAKey);

                        sql.Fill(result);

                        schema = (from p in parameters
                                  join s in result.AsEnumerable() on p.ColumnName equals s.Field<string>("COLUMN_NAME")
                                  select new Schema()
                                  {
                                      ColumnName = s.Field<string>("COLUMN_NAME"),
                                      DataType = s.Field<string>("DATA_TYPE"),
                                      Size = s.Field<object>("CHARACTER_OCTET_LENGTH")
                                  }).ToList();
                    }
                }

                //experimenting with MySql.Data library to process batch operation
                //as testing batch ops with MySqlConnector currently benchmarks
                //at 50K around 10 minutes... whereas MySql.Data runs a
                //full 100k around 1 minute...

                using (var c = new MySqlDataBatch.MySqlConnection(_connection))
                using (var adapter = new MySqlDataBatch.MySqlDataAdapter())
                using (var command = new MySqlDataBatch.MySqlCommand($"INSERT INTO {map.TableName} ({string.Join(",", schema.Select(x => x.ColumnName))}) VALUES ({string.Join(",", schema.Select(x => $"@{x.ColumnName}"))});", c))
                {
                    command.UpdatedRowSource = UpdateRowSource.None;

                    foreach (var type in schema)
                    {
                        var parameter = new MySqlDataBatch.MySqlParameter();
                        parameter.ParameterName = $"@{type.ColumnName}";
                        parameter.SourceColumn = type.ColumnName;

                        switch (type.DataType.ToLower())
                        {
                            case "varchar":
                            case "char":
                            case "text":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.String;
                                parameter.Size = Int32.Parse(type.Size.ToString());
                                break;
                            case "datetime":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.DateTime;
                                break;
                            case "int":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.Int32;
                                break;
                            case "bigint":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.Int64;
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        command.Parameters.Add(parameter);
                    }

                    adapter.InsertCommand = command;

                    var timer = Stopwatch.StartNew();

                    _log.LogTrace<MySqlQuery>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                    timer.Start();

                    if (batch > 0)
                        adapter.UpdateBatchSize = 100;

                    adapter.Update(table);

                    _log.LogTrace<MySqlQuery>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
                }
            });
        }

        public async Task InsertBatchAsync<T>(string connection, List<T> models, int batch = 0) where T : class, IModel
        {
            await Task.Run(() => {

                var mysql = new MySqlData.MySqlConnectionStringBuilder(connection);
                var database = mysql.Database;

                var map = _sql?.Configuration?.GetMap<T>();
                if (map == null)
                    throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");

                var name = map.TableName;
                var table = models.ToDataTable();
                if (table.Rows.Count == 0)
                    return;

                var builder = new StringBuilder();
                builder.Append("SELECT TABLE_NAME");
                builder.Append(", COLUMN_NAME");
                builder.Append(", DATA_TYPE");
                builder.Append(", CHARACTER_MAXIMUM_LENGTH");
                builder.Append(", CHARACTER_OCTET_LENGTH");
                builder.Append(", NUMERIC_PRECISION");
                builder.Append(", NUMERIC_SCALE AS SCALE");
                builder.Append(", COLUMN_DEFAULT");
                builder.Append(", IS_NULLABLE");
                builder.Append(" FROM INFORMATION_SCHEMA.COLUMNS");
                builder.Append(" WHERE TABLE_NAME = @Table");
                builder.Append(" AND TABLE_SCHEMA = @Database");

                var schema = new List<Schema>();

                //get table schema (e.g. names and datatypes for mapping)
                using (var c = new MySqlData.MySqlConnection(connection))
                using (var command = new MySqlData.MySqlCommand(builder.ToString(), c))
                {
                    var parameter = new MySqlData.MySqlParameter();
                    parameter.Value = map.TableName;
                    parameter.ParameterName = "@Table";
                    parameter.MySqlDbType = MySqlData.MySqlDbType.String;

                    var parameter2 = new MySqlData.MySqlParameter();
                    parameter2.Value = database;
                    parameter2.ParameterName = "@Database";
                    parameter2.MySqlDbType = MySqlData.MySqlDbType.String;

                    command.Parameters.Add(parameter);
                    command.Parameters.Add(parameter2);

                    using (var sql = new MySqlData.MySqlDataAdapter(command))
                    {
                        var result = new DataTable();
                        var parameters = map.Properties
                            .Where(x => x.Ignored == false)
                            .Where(x => x.IsReadOnly == false)
                            .Where(x => x.KeyType == KeyType.NotAKey);

                        sql.Fill(result);

                        schema = (from p in parameters
                                  join s in result.AsEnumerable() on p.ColumnName equals s.Field<string>("COLUMN_NAME")
                                  select new Schema()
                                  {
                                      ColumnName = s.Field<string>("COLUMN_NAME"),
                                      DataType = s.Field<string>("DATA_TYPE"),
                                      Size = s.Field<object>("CHARACTER_OCTET_LENGTH")
                                  }).ToList();
                    }
                }

                //experimenting with MySql.Data library to process batch operation
                //as testing batch ops with MySqlConnector currently benchmarks
                //at 50K around 10 minutes... whereas MySql.Data runs a
                //full 100k around 1 minute...

                using (var c = new MySqlDataBatch.MySqlConnection(connection))
                using (var adapter = new MySqlDataBatch.MySqlDataAdapter())
                using (var command = new MySqlDataBatch.MySqlCommand($"INSERT INTO {map.TableName} ({string.Join(",", schema.Select(x => x.ColumnName))}) VALUES ({string.Join(",", schema.Select(x => $"@{x.ColumnName}"))});", c))
                {
                    command.UpdatedRowSource = UpdateRowSource.None;

                    foreach (var type in schema)
                    {
                        var parameter = new MySqlDataBatch.MySqlParameter();
                        parameter.ParameterName = $"@{type.ColumnName}";
                        parameter.SourceColumn = type.ColumnName;

                        switch (type.DataType.ToLower())
                        {
                            case "varchar":
                            case "char":
                            case "text":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.String;
                                parameter.Size = Int32.Parse(type.Size.ToString());
                                break;
                            case "datetime":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.DateTime;
                                break;
                            case "int":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.Int32;
                                break;
                            case "bigint":
                                parameter.MySqlDbType = MySqlDataBatch.MySqlDbType.Int64;
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        command.Parameters.Add(parameter);
                    }

                    adapter.InsertCommand = command;

                    var timer = Stopwatch.StartNew();

                    _log.LogTrace<MySqlQuery>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                    timer.Start();

                    if (batch > 0)
                        adapter.UpdateBatchSize = 100;

                    adapter.Update(table);

                    _log.LogTrace<MySqlQuery>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
                }
            });
        }
    }
}
