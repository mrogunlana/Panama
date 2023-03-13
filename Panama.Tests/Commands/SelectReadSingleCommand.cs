using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System;
using System.Text;

namespace Panama.Tests.Commands
{
    public class SelectReadSingleCommand : ICommand
    {
        private readonly IMySqlQuery _query;
        private readonly string _connection;
        public SelectReadSingleCommand(IMySqlQuery query)
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
            builder.Append("select u.* from User u where u._ID = @_ID; ");

            definition.Sql = builder.ToString();
            definition.Parameters = new { user.ID };
            definition.Token = subject.Token;
            definition.Connection = _connection;

            var result = _query.GetSingle<User>(definition);

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}
