using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.Contexts;
using Panama.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Tests.Commands.EF
{
    public class DeleteUsers : ICommand
    {
        private readonly MySqlDbContext _context;

        public DeleteUsers(MySqlDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            var users = context.DataGet<User>();

            using (_context)
            {
                _context.RemoveRange(users);

                await _context.SaveChangesAsync();
            }
        }
    }
}
