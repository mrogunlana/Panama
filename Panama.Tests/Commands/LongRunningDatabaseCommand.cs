using DapperExtensions;
using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System;

namespace Panama.Tests.Commands
{
    public class LongRunningDatabaseCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public LongRunningDatabaseCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var definition = new Definition();

            definition.Sql = "select x.count from (select sleep(10) as sleep, count(u.ID) as count from User u) x;"; //10 seconds
            definition.Token = subject.Token;

            var result = _query.ExecuteScalar<int>(definition);

            subject.Context.Add(new KeyValuePair($"Count_{new Random().Next()}", result));
        }
    }
}
