using DapperExtensions;
using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Tests.Commands
{
    public class RollbackInsertCommand : IRollback
    {
        private readonly IMySqlQuery _query;

        public RollbackInsertCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public async Task Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;
            definition.Predicate = Predicates.Field<User>(x => x._ID, Operator.Eq, user._ID);

            _query.Delete(user, definition);

            await Task.FromResult(true);
        }
    }
}
