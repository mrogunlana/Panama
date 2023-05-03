using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;
using Panama.Canal.Tests.MySQL.Contexts;
using Panama.Canal.Tests.MySQL.Models;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL.Commands.EF
{
    public class SaveGeneratedUserInlineAndPublishWithEF : ICommand
    {
        private readonly IGenericChannelFactory _factory;
        private readonly AppDbContext _context;

        public SaveGeneratedUserInlineAndPublishWithEF(
              IGenericChannelFactory factory
            , AppDbContext context)
        {
            _factory = factory;
            _context = context;
        }
        public async Task Execute(IContext context)
        {
            using (var channel = _factory.CreateChannel<DatabaseFacade, IDbContextTransaction>(_context.Database, context.Token))
            {
                var user = new User {
                    ID = System.Guid.NewGuid().ToString(),
                    Email = $"test.{new System.Random().Next()}",
                    FirstName = $"first.{new System.Random().Next()}",
                    LastName = $"last.{new System.Random().Next()}",
                    Created = System.DateTime.UtcNow
                };

                _context.Add(user);
                _context.SaveChanges();

                await context.Bus()
                    .Channel(channel)
                    .Target<DefaultTarget>()
                    .Token(context.Token)
                    .Topic("foo.event")
                    .Group("foo")
                    .Data(user)
                    .Reply("foo.event.success")
                    .Post();

                await context.Bus()
                    .Channel(channel)
                    .Token(context.Token)
                    .Topic("bar.event")
                    .Group("bar")
                    .Data(user)
                    .Reply("bar.event.success")
                    .Post();

                await channel.Commit();
                
                context.Data.Add(user);
            }
        }
    }
}
