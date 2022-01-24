using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System;
using System.Text;

namespace Panama.Core.Tests.Commands
{
    public class SelectReadMultipleCommand : ICommand
    {
        private readonly IMySqlQuery _query;
        private readonly string _connection;
        public SelectReadMultipleCommand(IMySqlQuery query)
        {
            _query = query;
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};AllowUserVariables=True;";
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            var builder = new StringBuilder();
            builder.Append("set @_ID = (select u._ID from User u where u._ID = 1 limit 1); ");
            builder.Append("set @Value = 'test-value'; ");
            builder.Append("select @Value as `TestValue`, u.* from User u; ");

            definition.Connection = _connection;
            definition.Sql = builder.ToString();
            definition.Parameters = new { user.ID };
            definition.Token = subject.Token;

            var result = _query.Get<User>(definition);

            subject.Context.Remove(user);
            subject.Context.AddRange(result);
        }
    }
}
