using Panama.Extensions;
using Panama.Interfaces;
using Panama.Canal.Tests.MySQL.Contexts;
using Panama.Canal.Tests.MySQL.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Panama.Canal.Tests.MySQL.Commands.EF
{
    public class DeleteAllUsers : ICommand
    {
        private readonly AppDbContext _context;

        public DeleteAllUsers(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            _context.Database.ExecuteSqlRaw("TRUNCATE TABLE User");
        }
    }
}
