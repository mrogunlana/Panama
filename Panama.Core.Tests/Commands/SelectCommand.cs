using DapperExtensions;
using DapperExtensions.Sql;
using Org.BouncyCastle.Cms;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System.Collections.Generic;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SelectCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public SelectCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Sql = "select u.* from User u where u.ID = @ID;";
            definition.Parameters = new { user.ID };
            definition.Token = subject.Token;

            var result = _query.GetSingle<User>(definition);

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}
