using Panama.Extensions;
using Panama.Interfaces;
using Panama.Canal.Tests.MySQL.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Panama.Canal.Tests.MySQL.Commands.EF
{
    public class GetUsers : IQuery
    {
        private readonly AppDbContext _context;

        public GetUsers(AppDbContext context)
        {
            _context = context;
        }
        public Task Execute(IContext context)
        {
            var list = context.KvpGet<string, Guid>("ID");

            try
            {
                var users = _context.Users.Where(x => list.Contains(x.ID)).ToList();

                context.Data.AddRange(users);
            }
            catch (System.Exception ex)
            {
                var e = ex;
                throw;
            }
            

            return Task.CompletedTask;
        }
    }
}
