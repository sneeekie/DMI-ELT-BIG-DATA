using Microsoft.EntityFrameworkCore;
using Transform.Entities;

namespace Transform
{
    public class EfDbContext : DbContext
    {
        public DbSet<WeatherDataModel> WeatherDataModels { get; set; }

        private readonly string _connectionString;
        
        public EfDbContext(DbContextOptions<EfDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _connectionString = configuration["ConnectionStrings:Mssql"] ??
                                throw new ArgumentNullException(nameof(configuration));
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder
                .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information)
                .EnableSensitiveDataLogging(true)
                .UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeatherDataModel>().ToTable("WeatherData");
            modelBuilder.Entity<WeatherDataModel>().HasKey(w => w.Id);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Id);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Latitude);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Longitude);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Created);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Observed);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.ParameterId);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.StationId);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.Value);
            modelBuilder.Entity<WeatherDataModel>().Property(w => w.DmiID);
        }
    }
}