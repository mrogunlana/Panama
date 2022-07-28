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

namespace Panama.SqlServer.Dapper
{
    public class SqlServerQuery : IQuery
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public SqlServerQuery(ILog log,
            ISqlGenerator sql)
        {
            _log = log;
            _sql = sql;
            _connection = ConfigurationManager.AppSettings["Database"];

            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD")};";
        }

        public List<T> Get<T>(string sql, object parameters)
        {
            var result = new List<T>();
            
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlServerQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

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
                _log.LogTrace<SqlServerQuery>($"SELECT: {sql}. Connection: {connection}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                c.Open();

                result = c.Query<T>(sql, parameters).ToList();

                c.Close();
            }

            return result.ToList();
        }

        public void Insert<T>(T obj) where T : class
        {
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlServerQuery>($"INSERT: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                connection.Open();
                var key = connection.Insert(obj);
                connection.Close();
                
                obj.SetKey((object)key);
            }
        }

        public void Insert<T>(string connection, T obj) where T : class
        {
            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlServerQuery>($"INSERT: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

                c.Open();
                var key = c.Insert(obj);
                c.Close();

                obj.SetKey((object)key);
            }
        }

        public void Update<T>(T obj) where T : class
        {
            using (var connection = new SqlConnection(_connection))
            {
                _log.LogTrace<SqlServerQuery>($"UPDATE: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                connection.Open();
                connection.Update(obj);
                connection.Close();
            }
        }

        public void Update<T>(string connection, T obj) where T : class
        {
            using (var c = new SqlConnection(connection))
            {
                _log.LogTrace<SqlServerQuery>($"UPDATE: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

                c.Open();
                c.Update(obj);
                c.Close();
            }
        }

        public void Save<T>(T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = Get<T>($"select * from `{ _sql.Configuration.GetMap<T>().TableName }` where {properties}", parameters);
            if (exist.Count == 0)
                Insert(obj);
            else
                Update(obj);
        }

        public void Save<T>(string connection, T obj, object parameters) where T : class, IModel
        {
            var properties = string.Join(" AND ", parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            var exist = Get<T>(connection, $"select * from `{ _sql.Configuration.GetMap<T>().TableName }` where {properties}", parameters);
            if (exist.Count == 0)
                Insert(connection, obj);
            else
                Update(connection, obj);
        }

        bool IQuery.Exist<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        bool IQuery.Exist<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public T GetSingle<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public T GetSingle<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        void IQuery.Delete<T>(T obj)
        {
            throw new NotImplementedException();
        }

        void IQuery.Delete<T>(string connection, T obj)
        {
            throw new NotImplementedException();
        }

        public void Execute(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public void Execute(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public T ExecuteScalar<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public T ExecuteScalar<T>(string connection, string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        void IQuery.InsertBatch<T>(List<T> models, int batch)
        {
            throw new NotImplementedException();
        }

        void IQuery.InsertBatch<T>(string connection, List<T> models, int batch)
        {
            throw new NotImplementedException();
        }
    }
}
