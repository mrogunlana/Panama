using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class SelectAllCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public SelectAllCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var definition = new Definition();

            definition.Sql = "select u.* from User u;";
            definition.Token = subject.Token;

            var result = _query.Get<User>(definition);

            subject.Context.RemoveAll<User>();
            subject.Context.AddRange(result);
        }
    }
}
