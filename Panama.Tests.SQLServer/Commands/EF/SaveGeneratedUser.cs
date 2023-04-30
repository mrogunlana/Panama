using Panama.Interfaces;
using Panama.Tests.SQLServer.Contexts;
using Panama.Tests.SQLServer.Models;
using System.Threading.Tasks;

namespace Panama.Tests.SQLServer.Commands.EF
{
    public class SaveGeneratedUser : ICommand
    {
        private readonly AppDbContext _context;

        public SaveGeneratedUser(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            var user = new User {
                ID = System.Guid.NewGuid().ToString(),
                Email = $"test.{new System.Random().Next()}",
                FirstName = $"first.{new System.Random().Next()}",
                LastName = $"last.{new System.Random().Next()}",
                Created = System.DateTime.UtcNow
            };

            _context.Add(user);

            await _context.SaveChangesAsync();

            context.Data.Add(user);
        }
    }
}
