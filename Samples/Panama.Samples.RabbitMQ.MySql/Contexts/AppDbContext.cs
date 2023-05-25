using Microsoft.EntityFrameworkCore;
using Panama.Samples.RabbitMQ.MySql.Models;

namespace Panama.Samples.RabbitMQ.MySQL.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<WeatherForecast> Forecasts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherForecast>(builder => {
                builder.ToTable("Forcast");
                builder.HasKey(e => e._Id);
                builder.Ignore(e => e.TemperatureF);
                builder.Property(e => e.Id)
                    .HasColumnType("char(36)")
                    .HasMaxLength(36);
            });
        }
    }
}
