using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.Contexts;
using Panama.Tests.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Panama.Tests.Commands.EF
{
    public class GetUsers : IQuery
    {
        private readonly MySqlDbContext _context;

        public GetUsers(MySqlDbContext context)
        {
            _context = context;
        }
        public Task Execute(IContext context)
        {
            using (_context)
            {
                var users = _context.Users.Select(x => x).ToList();

                context.Data.AddRange(users);
            }

            return Task.CompletedTask;
        }
    }
}
