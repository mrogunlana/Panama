using Microsoft.EntityFrameworkCore;
using Panama.Tests.SQLServer.Models;

namespace Panama.Tests.SQLServer.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder => {
                builder.ToTable("User");
                builder.HasKey(u => u._ID);
            });
            modelBuilder.Entity<Setting>(builder => {
                builder.ToTable("Setting");
                builder.HasKey(s => s._ID);
                builder.Property(s => s.Key).HasMaxLength(25);
                builder.Property(s => s.Value).HasMaxLength(25);
            });
        }
    }
}
