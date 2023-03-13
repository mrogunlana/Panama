using Panama.Commands;
using Panama.Entities;
using Panama.Sql;
using Panama.Tests.Models;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Tests.Commands
{
    public class SelectReadSingleCommandUsingMssql : ICommandAsync
    {
        private readonly IQueryAsync _query;
        public SelectReadSingleCommandUsingMssql(IQueryAsync query)
        {
            _query = query;
        }
        public async Task Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();

            var builder = new StringBuilder();
            builder.Append("select u.* from [User] u where u.ID = @ID; ");

            var result = await _query.GetSingleAsync<User>(builder.ToString(), new { user.ID });

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}
