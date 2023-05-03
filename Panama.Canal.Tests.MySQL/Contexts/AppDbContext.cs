using Microsoft.EntityFrameworkCore;
using Panama.Canal.Tests.MySQL.Models;

namespace Panama.Canal.Tests.MySQL.Contexts
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
                builder.Property(u => u.ID)
                    .HasColumnType("char(36)")
                    .HasMaxLength(36);
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
