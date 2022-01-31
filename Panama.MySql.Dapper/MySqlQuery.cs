using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Newtonsoft.Json;
using Panama.Core.Entities;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MySqlData = MySqlConnector;

namespace Panama.Core.MySql.Dapper
{
    //todo: expose Task on CRUD ops now that Panama supports async commands
    public class MySqlQuery : IMySqlQuery
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;
        private readonly string _connection;

        public MySqlQuery(ILog log,
            ISqlGenerator sql)
        {
            _log = log;
            _sql = sql;
            _connection = ConfigurationManager.AppSettings["Database"];

            if (string.IsNullOrEmpty(_connection))
                _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};";
        }

        public List<T> Get<T>(string sql, object parameters)
        {
            var result = new List<T>();
            
            //var test = new MySql.Data.MySqlClient()
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.Query<T>(sql, parameters).ToList();

                connection.Close();
            }

            return result.ToList();
        }

        public List<T> Get<T>(Definition definition)
        {
            var result = new List<T>();
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;
            
            using (var mysql = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {definition.Sql}. Connection: {connection}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                mysql.Open();

                result = mysql.Query<T>(new CommandDefinition(definition.Sql, definition.Parameters, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout)).ToList();

                mysql.Close();
            }

            return result.ToList();
        }

        public List<T> Get<T>(string connection, string sql, object parameters)
        {
            var result = new List<T>();

            //var test = new MySql.Data.MySqlClient()
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {sql}. Connection: {connection}. Parameters: {JsonConvert.SerializeObject(parameters)}");

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

        public T GetSingle<T>(Definition definition)
        {
            return Get<T>(definition).FirstOrDefault();
        }

        public void Insert<T>(T obj) where T : class
        {
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"INSERT: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                connection.Open();
                var key = connection.Insert(obj);
                connection.Close();
                
                obj.SetKey((object)key);
            }
        }

        public void Insert<T>(string connection, T obj) where T : class
        {
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"INSERT: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

                c.Open();
                var key = c.Insert(obj);
                c.Close();

                obj.SetKey((object)key);
            }
        }

        public void Insert<T>(T obj, Definition definition) where T : class
        {
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                c.Open();

                //NOTE: we can not use DapperExtensions here as they do not support cancellation tokens
                var sql = _sql.Insert(_sql.Configuration.GetMap<T>());
                var builder = new StringBuilder();

                builder.Append($"{sql}; ");
                builder.Append("SELECT CONVERT(LAST_INSERT_ID(), SIGNED INTEGER) AS ID; ");

                var command = new CommandDefinition(builder.ToString(), obj, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout);

                _log.LogTrace<MySqlQuery>($"INSERT: {definition.Sql}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                var key = c.ExecuteScalar(command);

                c.Close();

                obj.SetKey(key);
            }
        }

        public void Update<T>(T obj) where T : class
        {
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"UPDATE: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                connection.Open();
                connection.Update(obj);
                connection.Close();
            }
        }

        public void Update<T>(string connection, T obj) where T : class
        {
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"UPDATE: {nameof(obj)}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}");

                c.Open();
                c.Update(obj);
                c.Close();
            }
        }

        public void Update<T>(T obj, Definition definition) where T : class
        {
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                c.Open();

                //NOTE: we can not use DapperExtensions here as they do not support cancellation tokens
                var sql = _sql.Update(_sql.Configuration.GetMap<T>(), definition.Predicate, definition.Dictionary);
                var parameters = new DynamicParameters();
                var map = _sql.Configuration.GetMap<T>();
                var columns = map.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));

                foreach (var property in ReflectionHelper.GetObjectValues(obj).Where(property => columns.Any(c => c.Name == property.Key)))
                    parameters.Add(property.Key, property.Value);

                foreach (var parameter in definition.Dictionary)
                    parameters.Add(parameter.Key, parameter.Value);

                var command = new CommandDefinition(sql, parameters, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout);

                _log.LogTrace<MySqlQuery>($"UPDATE: {definition.Sql}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                c.ExecuteScalar(command);
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

        public void Save<T>(T obj, Definition definition) where T : class, IModel
        {
            var properties = string.Join(" AND ", definition.Parameters.GetType().GetProperties().Select(x => $"{x.Name} = @{x.Name}"));
            
            var gist = new Definition();
            gist.Sql = $"select * from `{ _sql.Configuration.GetMap<T>().TableName }` where {properties}";
            gist.Token = definition.Token;
            gist.Parameters = definition.Parameters;
            gist.CommandTimeout = definition.CommandTimeout;
            gist.Connection = definition.Connection;

            var exist = Get<T>(gist);
            if (exist.Count == 0)
                Insert(obj, definition);
            else
                Update(obj, definition);
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

        public bool Exist<T>(Definition definition) where T : class, IModel
        {
            var exist = Get<T>(definition);
            if (exist.Count == 0)
                return false;

            return true;
        }

        public void Delete<T>(T obj) where T : class, IModel
        {
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"DELETE: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                connection.Open();
                connection.Delete(obj);
                connection.Close();
            }
        }

        public void Delete<T>(string connection, T obj) where T : class, IModel
        {
            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"DELETE: {nameof(obj)}. Object: {JsonConvert.SerializeObject(obj)}");

                c.Open();
                c.Delete(obj);
                c.Close();
            }
        }

        public void Delete<T>(T obj, Definition definition) where T : class, IModel
        {
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                c.Open();

                //NOTE: we can not use DapperExtensions here as they do not support cancellation tokens
                var sql = _sql.Delete(_sql.Configuration.GetMap<T>(), definition.Predicate, definition.Dictionary);
                var parameters = new DynamicParameters();
                var map = _sql.Configuration.GetMap<T>();
                var columns = map.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));

                foreach (var property in ReflectionHelper.GetObjectValues(obj).Where(property => columns.Any(c => c.Name == property.Key)))
                    parameters.Add(property.Key, property.Value);

                foreach (var parameter in definition.Dictionary)
                    parameters.Add(parameter.Key, parameter.Value);

                var command = new CommandDefinition(sql, parameters, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout);

                _log.LogTrace<MySqlQuery>($"DELETE: {definition.Sql}. Connection: {connection}. Object: {JsonConvert.SerializeObject(obj)}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                var result = c.ExecuteScalar<T>(command);
            }
        }

        public void Execute(string sql, object parameters)
        {
            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();
                connection.Execute(sql, parameters);
                connection.Close();
            }
        }

        public void Execute(string connection, string sql, object parameters)
        {
            using (var mysql = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                mysql.Open();
                mysql.Execute(sql, parameters);
                mysql.Close();
            }
        }

        public void Execute(Definition definition)
        {
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;

            using (var mysql = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {definition.Sql}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                mysql.Open();
                mysql.Execute(new CommandDefinition(definition.Sql, definition.Parameters, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout));
                mysql.Close();
            }
        }

        public T ExecuteScalar<T>(string sql, object parameters)
        {
            T result = default;

            using (var connection = new MySqlData.MySqlConnection(_connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.ExecuteScalar<T>(sql, parameters);

                connection.Close();
            }

            return result;
        }

        public T ExecuteScalar<T>(string connection, string sql, object parameters)
        {
            T result = default;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                c.Open();

                result = c.ExecuteScalar<T>(sql, parameters);

                c.Close();
            }

            return result;
        }

        public T ExecuteScalar<T>(Definition definition)
        {
            var connection = definition.Connection;
            if (string.IsNullOrEmpty(connection))
                connection = _connection;

            T result = default;

            using (var c = new MySqlData.MySqlConnection(connection))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {definition.Sql}. Parameters: {JsonConvert.SerializeObject(definition.Parameters)}");

                c.Open();

                result = c.ExecuteScalar<T>(new CommandDefinition(definition.Sql, definition.Parameters, cancellationToken: definition.Token, commandTimeout: definition.CommandTimeout));

                c.Close();
            }

            return result;
        }

        public void InsertBatch<T>(List<T> models, int batch = 0) where T : class, IModel
        {
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

            using (var c = new MySqlData.MySqlConnection(_connection))
            using (var adapter = new MySqlData.MySqlDataAdapter())
            using (var command = new MySqlData.MySqlCommand($"INSERT INTO {map.TableName} ({string.Join(",", schema.Select(x => x.ColumnName))}) VALUES ({string.Join(",", schema.Select(x => $"@{x.ColumnName}"))});", c))
            {
                command.UpdatedRowSource = UpdateRowSource.None;

                foreach (var type in schema)
                {
                    var parameter = new MySqlData.MySqlParameter();
                    parameter.ParameterName = $"@{type.ColumnName}";
                    parameter.SourceColumn = type.ColumnName;

                    switch (type.DataType.ToLower())
                    {
                        case "varchar":
                        case "char":
                        case "text":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.String;
                            parameter.Size = Int32.Parse(type.Size.ToString());
                            break;
                        case "datetime":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.DateTime;
                            break;
                        case "int":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.Int32;
                            break;
                        case "bigint":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.Int64;
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
        }

        public void InsertBatch<T>(string connection, List<T> models, int batch = 0) where T : class, IModel
        {
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

            using (var c = new MySqlData.MySqlConnection(connection))
            using (var adapter = new MySqlData.MySqlDataAdapter())
            using (var command = new MySqlData.MySqlCommand($"INSERT INTO {map.TableName} ({string.Join(",", schema.Select(x => x.ColumnName))}) VALUES ({string.Join(",", schema.Select(x => $"@{x.ColumnName}"))});", c))
            {
                command.UpdatedRowSource = UpdateRowSource.None;

                foreach (var type in schema)
                {
                    var parameter = new MySqlData.MySqlParameter();
                    parameter.ParameterName = $"@{type.ColumnName}";
                    parameter.SourceColumn = type.ColumnName;

                    switch (type.DataType.ToLower())
                    {
                        case "varchar":
                        case "char":
                        case "text":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.String;
                            parameter.Size = Int32.Parse(type.Size.ToString());
                            break;
                        case "datetime":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.DateTime;
                            break;
                        case "int":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.Int32;
                            break;
                        case "bigint":
                            parameter.MySqlDbType = MySqlData.MySqlDbType.Int64;
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
        }
    }
}
