using Panama.Core.Commands;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System;
using System.Collections.Generic;

namespace Panama.Core.Tests.Commands
{
    public class InsertCommandSomeBatchRandomUsersOnDifferentDatabase : ICommand
    {
        private readonly IMySqlQuery _query;
        private readonly string _connection;

        public InsertCommandSomeBatchRandomUsersOnDifferentDatabase(IMySqlQuery query)
        {
            _query = query;
            _connection = $"Server={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_SERVER")};Port={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PORT")};Database={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_DATABASE_TEMP")};Uid={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_USER")};Pwd={Environment.GetEnvironmentVariable("ASPNETCORE_MYSQL_PASSWORD")};AllowUserVariables=True;";
        }
        public void Execute(Subject subject)
        {
            var list = new List<User>() {
                new User() {
                    ID = System.Guid.NewGuid(),
                    Email = $"test.{System.Guid.NewGuid().ToString().Replace("-", "")}@test.com",
                    FirstName = "test1",
                    LastName = "test"
                },
                new User() {
                    ID = System.Guid.NewGuid(),
                    Email = $"test.{System.Guid.NewGuid().ToString().Replace("-", "")}@test.com",
                    FirstName = "test2",
                    LastName = "test"
                }
            };
            var definition = new Definition();

            definition.Token = subject.Token;

            _query.InsertBatch(_connection, list);
        }
    }
}
