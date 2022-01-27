using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System;

namespace Panama.Core.Tests.Commands
{
    public class SaveCommand : ICommand
    {
        private readonly IMySqlQuery _query;
        private readonly string _connection;

        public SaveCommand(IMySqlQuery query)
        {
            _query = query;
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};AllowUserVariables=True;";
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;

            _query.Save(_connection, user, new { _ID = user._ID });
        }
    }
}
