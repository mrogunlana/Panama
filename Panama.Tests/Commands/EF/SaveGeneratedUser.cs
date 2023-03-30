using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.Contexts;
using Panama.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Tests.Commands.EF
{
    public class SaveGeneratedUser : ICommand
    {
        private readonly MySqlDbContext _context;

        public SaveGeneratedUser(MySqlDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            using (_context)
            {
                _context.Add(new User { 
                    ID = System.Guid.NewGuid(), 
                    Email = $"test.{new System.Random().Next()}",
                    FirstName = $"first.{new System.Random().Next()}",
                    LastName = $"last.{new System.Random().Next()}",
                    Created = System.DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}
