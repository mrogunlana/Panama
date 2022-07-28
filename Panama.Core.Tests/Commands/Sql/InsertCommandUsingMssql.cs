using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.Sql;
using Panama.Core.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Core.Tests.Commands
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
