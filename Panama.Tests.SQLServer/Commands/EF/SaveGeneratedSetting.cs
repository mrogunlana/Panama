using Panama.Interfaces;
using Panama.Tests.SQLServer.Contexts;
using Panama.Tests.SQLServer.Models;
using System.Threading.Tasks;

namespace Panama.Tests.SQLServer.Commands.EF
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
                Created = System.DateTime.Now
            });

            await _context.SaveChangesAsync();
        }
    }
}
