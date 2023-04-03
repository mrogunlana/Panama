using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;
using Panama.Tests.MySQL.Contexts;
using Panama.Tests.MySQL.Models;
using System.Threading.Tasks;

namespace Panama.Tests.MySQL.Commands.EF
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
                    Created = System.DateTime.Now
                };

                _context.Add(user);
                _context.SaveChanges();

                await channel.Post(
                    name: "foo.event",
                    group: "foo",
                    data: user,
                    ack: "foo.event.success",
                    nack: "foo.event.failed")
                    .ConfigureAwait(false);

                await channel.Post<DefaultTarget>(
                    name: "bar.event",
                    group: "bar",
                    data: user,
                    ack: "bar.event.success",
                    nack: "bar.event.failed")
                    .ConfigureAwait(false);

                await channel.Commit()
                    .ConfigureAwait(false);
                
                context.Data.Add(user);
            }
        }
    }
}
