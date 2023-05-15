using Panama.Interfaces;
using Panama.Canal.Tests.MySQL.Contexts;
using Panama.Canal.Tests.MySQL.Models;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL.Commands.EF
{
    public class SaveGeneratedSetting : ICommand
    {
        private readonly AppDbContext _context;

        public SaveGeneratedSetting(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            _context.Add(new Setting {
                ID = System.Guid.NewGuid(),
                Key = $"test.{new System.Random().Next()}",
                Value = $"value.{new System.Random().Next()}",
                Created = System.DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }
    }
}
