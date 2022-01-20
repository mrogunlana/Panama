using Panama.Core.Commands;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
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
