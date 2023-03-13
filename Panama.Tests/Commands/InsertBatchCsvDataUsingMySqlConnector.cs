using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using MySqlConnector;
using Panama.Commands;
using Panama.Entities;
using Panama.Logger;
using Panama.MySql.Dapper;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Panama.Tests.Commands
{
    public class InsertBatchCsvDataUsingMySqlConnector : ICommand
    {
        private readonly string _connection;
        private readonly ILog _log;
        private readonly ISqlGenerator _sql;

        public InsertBatchCsvDataUsingMySqlConnector(
              ILog log
            , ISqlGenerator sql)
        {
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};AllowUserVariables=True;";
            _log = log;
            _sql = sql;
        }
        public void Execute(Subject subject)
        {
            var models = subject.Context.DataGet<Csv>();
            var batch = subject.Context.KvpGetSingle<int>("Batch");

            var mysql = new MySqlConnectionStringBuilder(_connection);
            var database = mysql.Database;

            var map = _sql?.Configuration?.GetMap<Csv>();
            if (map == null)
                throw new Exception($"Class Map for:{typeof(Csv).Name} could not be found.");

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
            using (var c = new MySqlConnection(_connection))
            using (var command = new MySqlCommand(builder.ToString(), c))
            {
                var parameter = new MySqlParameter();
                parameter.Value = map.TableName;
                parameter.ParameterName = "@Table";
                parameter.MySqlDbType = MySqlDbType.String;

                var parameter2 = new MySqlParameter();
                parameter2.Value = database;
                parameter2.ParameterName = "@Database";
                parameter2.MySqlDbType = MySqlDbType.String;

                command.Parameters.Add(parameter);
                command.Parameters.Add(parameter2);

                using (var sql = new MySqlDataAdapter(command))
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

            using (var c = new MySqlConnection(_connection))
            using (var adapter = new MySqlDataAdapter())
            using (var command = new MySqlCommand($"INSERT INTO {map.TableName} ({string.Join(",", schema.Select(x => x.ColumnName))}) VALUES ({string.Join(",", schema.Select(x => $"@{x.ColumnName}"))});", c))
            {
                command.UpdatedRowSource = UpdateRowSource.None;

                foreach (var type in schema)
                {
                    var parameter = new MySqlParameter();
                    parameter.ParameterName = $"@{type.ColumnName}";
                    parameter.SourceColumn = type.ColumnName;

                    switch (type.DataType.ToLower())
                    {
                        case "varchar":
                        case "char":
                        case "text":
                            parameter.MySqlDbType = MySqlDbType.String;
                            parameter.Size = Int32.Parse(type.Size.ToString());
                            break;
                        case "datetime":
                            parameter.MySqlDbType = MySqlDbType.DateTime;
                            break;
                        case "int":
                            parameter.MySqlDbType = MySqlDbType.Int32;
                            break;
                        case "bigint":
                            parameter.MySqlDbType = MySqlDbType.Int64;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    command.Parameters.Add(parameter);
                }

                adapter.InsertCommand = command;

                var timer = Stopwatch.StartNew();

                _log.LogTrace<InsertBatchCsvDataUsingMySqlConnector>($"Bulk Insert on {name}. {models.Count} rows queued for insert.");

                timer.Start();

                adapter.UpdateBatchSize = batch;
                adapter.Update(table);

                _log.LogTrace<InsertBatchCsvDataUsingMySqlConnector>($"Bulk Insert on {name} complete in: {timer.Elapsed.ToString(@"hh\:mm\:ss\:fff")}");
            }
        }
    }
}
