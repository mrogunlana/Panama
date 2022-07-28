using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.Sql;
using Panama.Core.Tests.Models;
using System;
using System.Text;

namespace Panama.Core.Tests.Commands
{
    public class SelectReadSingleLogEntryCommand : ICommand
    {
        private readonly IQuery _query;
        private readonly string _connection;
        public SelectReadSingleLogEntryCommand(IQuery query)
        {
            _query = query;
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MSSQL_PASSWORD")};AllowUserVariables=True;";
        }
        public void Execute(Subject subject)
        {
            var log = subject.Context.DataGetSingle<Log>();

            var builder = new StringBuilder();
            builder.Append("set * from Log where Level = @Level ");

            var result = _query.GetSingle<User>(_connection, builder.ToString(), new { log.Level });

            subject.Context.RemoveAll<Log>();
            subject.Context.Add(result);
        }
    }
}
