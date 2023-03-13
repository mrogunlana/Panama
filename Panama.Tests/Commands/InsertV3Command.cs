using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System;

namespace Panama.Tests.Commands
{
    public class InsertV3Command : ICommand
    {
        private readonly IMySqlQuery _query;
        private readonly string _connection;

        public InsertV3Command(IMySqlQuery query)
        {
            _query = query;
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};AllowUserVariables=True;";
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;

            _query.Insert(_connection, user);
        }
    }
}
