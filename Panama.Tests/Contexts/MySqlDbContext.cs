using Microsoft.EntityFrameworkCore;
using Panama.Tests.Models;

namespace Panama.Tests.Contexts
{
    public class MySqlDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
