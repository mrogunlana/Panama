using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
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
