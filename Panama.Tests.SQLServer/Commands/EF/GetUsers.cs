using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.SQLServer.Contexts;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Tests.SQLServer.Commands.EF
{
    public class GetUsers : IQuery
    {
        private readonly AppDbContext _context;

        public GetUsers(AppDbContext context)
        {
            _context = context;
        }
        public Task Execute(IContext context)
        {
            var list = context.KvpGet<string, string>("ID");

            var users = _context.Users.Where(x => list.Contains(x.ID)).ToList();

            context.Data.AddRange(users);

            return Task.CompletedTask;
        }
    }
}
