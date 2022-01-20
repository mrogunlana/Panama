using DapperExtensions;
using DapperExtensions.Sql;
using Org.BouncyCastle.Cms;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class InsertCommandUsingExecuteSql : ICommand
    {
        private readonly IMySqlQuery _query;

        public InsertCommandUsingExecuteSql(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;

            var builder = new StringBuilder();

            builder.Append("insert into User (ID, FirstName, Created) ");
            builder.Append($"value (@ID, 'U:{System.DateTime.Now.ToShortDateString()}', now()); ");

            _query.Execute(builder.ToString(), new {
                ID = System.Guid.NewGuid()
            });
        }
    }
}
