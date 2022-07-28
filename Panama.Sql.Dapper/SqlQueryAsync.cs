using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Newtonsoft.Json;
using Panama.Core.Entities;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Core.Sql.Dapper
{
    public class SqlQueryAsync : IQueryAsync
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public SqlQueryAsync(ILog log)
        {
            _log = log;
            _sql = new SqlGeneratorImpl(new DapperExtensions.DapperExtensionsConfiguration());
            _connection = ConfigurationManager.AppSettings["Database"];

            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE")};User Id={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_USER")};Password={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD")};";
        }

        public async Task<List<T>> GetAsync<T>(string sql, object parameters)
        {
            var result = new List<T>();

            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQueryAsync>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

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
            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQueryAsync>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

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
                using (var connection = new SqlConnection(_connection))
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
                using (var c = new SqlConnection(connection))
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
                using (var connection = new SqlConnection(_connection))
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
                using (var c = new SqlConnection(connection))
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
                using (var connection = new SqlConnection(_connection))
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
                using (var c = new SqlConnection(connection))
                {
                    c.Open();
                    c.Delete(obj);
                    c.Close();
                }
            });
        }

        public async Task ExecuteAsync(string sql, object parameters)
        {
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQueryAsync>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await connection.OpenAsync();
                await connection.ExecuteAsync(sql, parameters);
                await connection.CloseAsync();
            }
        }

        public async Task ExecuteAsync(string connection, string sql, object parameters)
        {
            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQueryAsync>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();
                await c.ExecuteAsync(sql, parameters);
                await c.CloseAsync();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object parameters)
        {
            T result = default;

            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQueryAsync>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await connection.OpenAsync();

                result = await connection.ExecuteScalarAsync<T>(sql, parameters);

                await connection.CloseAsync();
            }

            return result;
        }

        public async Task<T> ExecuteScalarAsync<T>(string connection, string sql, object parameters)
        {
            T result = default;

            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQueryAsync>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();

                result = await c.ExecuteScalarAsync<T>(sql, parameters);

                await c.CloseAsync();
            }

            return result;
        }

        public async Task InsertBatchAsync<T>(List<T> models, int batch = 0) where T : class, IModel
        {
            await Task.Run(() => {

                using (var c = new SqlConnection(_connection))
                {
                    c.Open();

                    using (var transaction = c.BeginTransaction())
                    {
                        try
                        {
                            var map = _sql?.Configuration?.GetMap<T>();
                            if (map == null)
                                throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");

                            var name = map.TableName;
                            var table = models.ToDataTable();
                            if (table.Rows.Count == 0)
                                return;

                            foreach (var property in map.Properties)
                            {
                                if (property.Ignored)
                                    table.Columns.Remove(property.Name);
                                if (property.IsReadOnly)
                                    table.Columns.Remove(property.Name);
                                if (property.KeyType == KeyType.Identity)
                                    table.Columns.Remove(property.Name);
                            }

                            var timer = Stopwatch.StartNew();

                            _log.LogTrace<SqlQuery>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                            timer.Start();

                            using (var bulk = new SqlBulkCopy(c, SqlBulkCopyOptions.Default, transaction))
                            {
                                foreach (DataColumn column in table.Columns)
                                    bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                                if (batch > 0)
                                    bulk.BatchSize = batch;

                                bulk.DestinationTableName = $"[{name}]";
                                bulk.WriteToServer(table);
                            }

                            transaction.Commit();

                            _log.LogTrace<SqlQuery>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();

                            throw;
                        }
                        finally
                        {
                            c.Close();
                        }
                    }
                }

            });
        }

        public async Task InsertBatchAsync<T>(string connection, List<T> models, int batch = 0) where T : class, IModel
        {
            await Task.Run(() => {

                using (var c = new SqlConnection(connection))
                {
                    c.Open();

                    using (var transaction = c.BeginTransaction())
                    {
                        try
                        {
                            var map = _sql?.Configuration?.GetMap<T>();
                            if (map == null)
                                throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");

                            var name = map.TableName;
                            var table = models.ToDataTable();
                            if (table.Rows.Count == 0)
                                return;

                            foreach (var property in map.Properties)
                            {
                                if (property.Ignored)
                                    table.Columns.Remove(property.Name);
                                if (property.IsReadOnly)
                                    table.Columns.Remove(property.Name);
                                if (property.KeyType == KeyType.Identity)
                                    table.Columns.Remove(property.Name);
                            }

                            var timer = Stopwatch.StartNew();

                            _log.LogTrace<SqlQuery>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                            timer.Start();

                            using (var bulk = new SqlBulkCopy(c, SqlBulkCopyOptions.Default, transaction))
                            {
                                foreach (DataColumn column in table.Columns)
                                    bulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                                if (batch > 0)
                                    bulk.BatchSize = batch;

                                bulk.DestinationTableName = $"[{name}]";
                                bulk.WriteToServer(table);
                            }

                            transaction.Commit();

                            _log.LogTrace<SqlQuery>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();

                            throw;
                        }
                        finally
                        {
                            c.Close();
                        }
                    }
                }

            });
        }
    }
}
