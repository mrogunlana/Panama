using Dapper;
using Dapper.Contrib.Extensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Newtonsoft.Json;
using Panama.Core.Entities;
using Panama.Core.Logger;
using Panama.Core.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MySqlData = MySql.Data;

namespace Panama.MySql.Dapper
{
    public class MySqlQuery : IQuery
    {
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;

        public MySqlQuery(ILog log)
        {
            _log = log;
            _sql = new SqlGeneratorImpl(new DapperExtensions.DapperExtensionsConfiguration());
        }
        public List<T> Get<T>(string sql, object parameters)
        {
            var result = new List<T>();
            
            //var test = new MySql.Data.MySqlClient()
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                _log.LogTrace<MySqlQuery>($"SELECT: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.Query<T>(sql, parameters).ToList();

                connection.Close();
            }

            return result.ToList();
        }

        public T GetSingle<T>(string sql, object parameters)
        {
            return Get<T>(sql, parameters).FirstOrDefault();
        }

        public void Insert<T>(T obj) where T : class
        {
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                connection.Open();
                connection.Insert(obj);
                connection.Close();
            }
        }

        public void Update<T>(T obj) where T : class
        {
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                connection.Open();
                connection.Update(obj);
                connection.Close();
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

        public bool Exist<T>(string sql, object parameters) where T : class, IModel
        {
            var exist = Get<T>(sql, parameters);
            if (exist.Count == 0)
                return false;

            return true;
        }

        public void Delete<T>(T obj) where T : class, IModel
        {
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                connection.Open();
                connection.Delete(obj);
                connection.Close();
            }
        }

        public void Execute(string sql, object parameters)
        {
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();
                connection.Execute(sql, parameters);
                connection.Close();
            }
        }

        public T ExecuteScalar<T>(string sql, object parameters)
        {
            T result = default;

            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
            {
                _log.LogTrace<MySqlQuery>($"EXECUTE: {sql}. Parameters: {JsonConvert.SerializeObject(parameters)}");

                connection.Open();

                result = connection.ExecuteScalar<T>(sql, parameters);

                connection.Close();
            }

            return result;
        }

        public void InsertBatch<T>(List<T> models, int batch = 0) where T : class, IModel
        {
            using (var connection = new MySqlData.MySqlClient.MySqlConnection(ConfigurationManager.AppSettings["Database"]))
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

                        var builder = new StringBuilder();
                        var adapter = new MySqlData.MySqlClient.MySqlDataAdapter();
                        var parameters = map.Properties
                            .Where(x => x.Ignored == false)
                            .Where(x => x.IsReadOnly == false)
                            .Where(x => x.KeyType == KeyType.NotAKey);

                        builder.Append($"INSERT INTO {map.TableName} ({string.Join(",", parameters.Select(x => x.ColumnName))}) VALUES ({string.Join(",", parameters.Select(x => $"@{x.ColumnName}"))});");

                        adapter.InsertCommand = new MySqlData.MySqlClient.MySqlCommand(builder.ToString(), connection);
                        adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                        
                        //clear builder and build schema sql get
                        //builder.Clear();
                        //builder.Append("SELECT TABLE_NAME");
                        //builder.Append(", COLUMN_NAME");
                        //builder.Append(", DATA_TYPE");
                        //builder.Append(", CHARACTER_MAXIMUM_LENGTH");
                        //builder.Append(", CHARACTER_OCTET_LENGTH");
                        //builder.Append(", NUMERIC_PRECISION");
                        //builder.Append(", NUMERIC_SCALE AS SCALE");
                        //builder.Append(", COLUMN_DEFAULT");
                        //builder.Append(", IS_NULLABLE");
                        //builder.Append(" FROM INFORMATION_SCHEMA.COLUMNS");
                        //builder.Append($" WHERE TABLE_NAME = '{map.TableName}'");

                        //get table schema (e.g. names and datatypes for mapping)
                        using (var command = new MySqlData.MySqlClient.MySqlCommand($"SELECT * FROM {map.TableName}", connection))
                        using (var reader = command.ExecuteReader())
                        {
                            var schema = reader.GetSchemaTable();

                            var types = from p in parameters
                                        join s in schema.AsEnumerable() on p.ColumnName equals s.Field<string>("COLUMN_NAME")
                                        select new {
                                            ColumnName = s.Field<string>("COLUMN_NAME"), 
                                            DataType = s.Field<string>("DATA_TYPE"),
                                            Size = s.Field<int>("CHARACTER_MAXIMUM_LENGTH")
                                        };

                            foreach (var type in types)
                            {
                                switch (type.DataType.ToLower())
                                {
                                    case "varchar":
                                    case "char":
                                    case "text":
                                        adapter.InsertCommand.Parameters.Add($"@{type.ColumnName}", MySqlData.MySqlClient.MySqlDbType.String, type.Size);
                                        break;
                                    case "datetime":
                                        adapter.InsertCommand.Parameters.Add($"@{type.ColumnName}", MySqlData.MySqlClient.MySqlDbType.DateTime);
                                        break;
                                    case "int":
                                        adapter.InsertCommand.Parameters.Add($"@{type.ColumnName}", MySqlData.MySqlClient.MySqlDbType.Int32);
                                        break;
                                    default:
                                        break;
                                }
                                
                            }
                        }

                        var timer = Stopwatch.StartNew();

                        _log.LogTrace<MySqlQuery>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                        timer.Start();

                        if (batch > 0)
                            adapter.UpdateBatchSize = 100;

                        adapter.Update(table);

                        transaction.Commit();

                        _log.LogTrace<MySqlQuery>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
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
    }
}
