using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.SQLServer.Contexts;
using Panama.Tests.SQLServer.Models;
using System.Threading.Tasks;

namespace Panama.Tests.SQLServer.Commands.EF
{
    public class DeleteUsers : ICommand
    {
        private readonly AppDbContext _context;

        public DeleteUsers(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            var users = context.DataGet<User>();

            _context.RemoveRange(users);

            await _context.SaveChangesAsync();
        }
    }
}
