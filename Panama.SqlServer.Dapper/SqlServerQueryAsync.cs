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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using Panama.Core.Sql;
using System.Threading.Tasks;

namespace Panama.SqlServer.Dapper
{
    public class SqlServerQueryAsync : IQueryAsync
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public SqlServerQueryAsync(ILog log,
            ISqlGenerator sql)
        {
            _log = log;
            _sql = sql;
            _connection = ConfigurationManager.AppSettings["Database"];

            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD")};";
        }

        public async Task<List<T>> GetAsync<T>(string sql, object parameters)
        {
            var result = new List<T>();
            
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlServerQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

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

            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlServerQuery>($"SELECT: {sql}. Connection: {connection}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                await c.OpenAsync();

                var query = await c.QueryAsync<T>(sql, parameters);

                result = query.ToList();

                await c.CloseAsync();
            }

            return result.ToList();
        }

        public async Task InsertAsync<T>(T obj) where T : class
        {
            await Task.Run(() => {
                using (var connection = new SqlConnection(_connection))
                {
                    _log.LogTrace<SqlServerQuery>($"INSERT: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                    connection.Open();
                    var key = connection.Insert(obj);
                    connection.Close();
                    
                    obj.SetKey((object)key);
                }
            });
        }

        public async Task InsertAsync<T>(string connection, T obj) where T : class
        {
            await Task.Run(() => {
                using (var c = new SqlConnection(connection))
                {
                    _log.LogTrace<SqlServerQuery>($"INSERT: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

                    c.Open();
                    var key = c.Insert(obj);
                    c.Close();

                    obj.SetKey((object)key);
                }
            });
        }

        public async Task UpdateAsync<T>(T obj) where T : class
        {
            await Task.Run(() => {
                using (var connection = new SqlConnection(_connection))
                {
                    _log.LogTrace<SqlServerQuery>($"UPDATE: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

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
                    _log.LogTrace<SqlServerQuery>($"UPDATE: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

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

        Task<bool> IQueryAsync.ExistAsync<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        Task<bool> IQueryAsync.ExistAsync<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetSingleAsync<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetSingleAsync<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        Task IQueryAsync.DeleteAsync<T>(T obj)
        {
            throw new NotImplementedException();
        }

        Task IQueryAsync.DeleteAsync<T>(string connection, T obj)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteScalarAsync<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        Task IQueryAsync.InsertBatchAsync<T>(List<T> models, int batch)
        {
            throw new NotImplementedException();
        }

        Task IQueryAsync.InsertBatchAsync<T>(string connection, List<T> models, int batch)
        {
            throw new NotImplementedException();
        }
    }
}
