using Microsoft.EntityFrameworkCore;
using ChronosTracker.Core.Entities;

namespace ChronosTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
}
