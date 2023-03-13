using Panama.Commands;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
{
    public class InsertCommandANewRandomUser : ICommand
    {
        private readonly IMySqlQuery _query;

        public InsertCommandANewRandomUser(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var ID = System.Guid.NewGuid();
            var user = new User() {
                ID = ID,
                Email = $"test.{ID.ToString().Replace("-", "")}@test.com",
                FirstName = "test",
                LastName = "test"
            };
            var definition = new Definition();

            definition.Token = subject.Token;

            _query.Insert(user, definition);
        }
    }
}
