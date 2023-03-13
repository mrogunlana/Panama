using Panama.Commands;
using Panama.Entities;
using Panama.Sql;
using Panama.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Tests.Commands
{
    public class InsertCommandUsingMssql : ICommandAsync
    {
        private readonly IQueryAsync _query;

        public InsertCommandUsingMssql(IQueryAsync query)
        {
            _query = query;
        }
        public async Task Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();

            await _query.InsertAsync(user);
        }
    }
}
