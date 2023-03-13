using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
{
    public class SelectUserByEmailCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public SelectUserByEmailCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Sql = "select u.* from User u where u.Email = @Email;";
            definition.Parameters = new { user.Email };
            definition.Token = subject.Token;

            var result = _query.GetSingle<User>(definition);

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}
