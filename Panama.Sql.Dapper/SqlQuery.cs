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

namespace Panama.Core.Sql.Dapper
{
    public class SqlQuery : IQuery
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public SqlQuery(ILog log)
        {
            _log = log;
            _sql = new SqlGeneratorImpl(new DapperExtensions.DapperExtensionsConfiguration());
            _connection = ConfigurationManager.AppSettings["Database"];
            
            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE")};User Id={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_USER")};Password={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD")};";
        }
        public List<T> Get<T>(string sql, object parameters)
        {
            var result = new List<T>();

            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.Query<T>(sql, parameters).ToList();

                connection.Close();
            }

            return result.ToList();
        }
        public List<T> Get<T>(string connection, string sql, object parameters)
        {
            var result = new List<T>();

            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                c.Open();

                result = c.Query<T>(sql, parameters).ToList();

                c.Close();
            }

            return result.ToList();
        }

        public T GetSingle<T>(string sql, object parameters)
        {
            return Get<T>(sql, parameters).FirstOrDefault();
        }
        public T GetSingle<T>(string connection, string sql, object parameters)
        {
            return Get<T>(connection, sql, parameters).FirstOrDefault();
        }

        public void Insert<T>(T obj) where T : class
        {
            using (var connection = new SqlConnection(_connection))
            {
                connection.Open();
                connection.Insert(obj);
                connection.Close();
            }
        }
        public void Insert<T>(string connection, T obj) where T : class
        {
            using (var c = new SqlConnection(connection))
            {
                c.Open();
                c.Insert(obj);
                c.Close();
            }
        }

        public void Update<T>(T obj) where T : class
        {
            using (var connection = new SqlConnection(_connection))
            {
                connection.Open();
                connection.Update(obj);
                connection.Close();
            }
        }
        public void Update<T>(string connection, T obj) where T : class
        {
            using (var c = new SqlConnection(connection))
            {
                c.Open();
                c.Update(obj);
                c.Close();
            }
        }

        public void Save<T>(T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = Get<T>($"select * from [{ _sql.Configuration.GetMap<T>().TableName }] where {properties}", parameters);
            if (exist.Count == 0)
                Insert(obj);
            else
                Update(obj);
        }
        public void Save<T>(string connection, T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = Get<T>(connection, $"select * from [{ _sql.Configuration.GetMap<T>().TableName }] where {properties}", parameters);
            if (exist.Count == 0)
                Insert(connection, obj);
            else
                Update(connection, obj);
        }

        public bool Exist<T>(string sql, object parameters) where T : class, IModel
        {
            var exist = Get<T>(sql, parameters);
            if (exist.Count == 0)
                return false;

            return true;
        }
        public bool Exist<T>(string connection, string sql, object parameters) where T : class, IModel
        {
            var exist = Get<T>(connection, sql, parameters);
            if (exist.Count == 0)
                return false;

            return true;
        }

        public void Delete<T>(T obj) where T : class, IModel
        {
            using (var connection = new SqlConnection(_connection))
            {
                connection.Open();
                connection.Delete(obj);
                connection.Close();
            }
        }
        public void Delete<T>(string connection, T obj) where T : class, IModel
        {
            using (var c = new SqlConnection(connection))
            {
                c.Open();
                c.Delete(obj);
                c.Close();
            }
        }

        public void Execute(string sql, object parameters)
        {
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();
                connection.Execute(sql, parameters);
                connection.Close();
            }
        }
        public void Execute(string connection, string sql, object parameters)
        {
            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                c.Open();
                c.Execute(sql, parameters);
                c.Close();
            }
        }
        public T ExecuteScalar<T>(string sql, object parameters)
        {
            T result = default;

            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.ExecuteScalar<T>(sql, parameters);

                connection.Close();
            }

            return result;
        }
        public T ExecuteScalar<T>(string connection, string sql, object parameters)
        {
            T result = default;

            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                c.Open();

                result = c.ExecuteScalar<T>(sql, parameters);

                c.Close();
            }

            return result;
        }

        public void InsertBatch<T>(List<T> models, int batch = 0) where T : class, IModel
        {
            using (var connection = new SqlConnection(_connection))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
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

                        using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
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
                        connection.Close();
                    }
                }
            }
        }
        public void InsertBatch<T>(string connection, List<T> models, int batch = 0) where T : class, IModel
        {
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
        }
    }
}
