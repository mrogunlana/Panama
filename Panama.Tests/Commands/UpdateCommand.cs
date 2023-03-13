using DapperExtensions;
using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
{
    public class UpdateCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public UpdateCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;
            definition.Predicate = Predicates.Field<User>(x => x._ID, Operator.Eq, user._ID);

            _query.Update(user);
        }
    }
}
