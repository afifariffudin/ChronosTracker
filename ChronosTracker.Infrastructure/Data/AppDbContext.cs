using Microsoft.EntityFrameworkCore;
using ChronosTracker.Core.Entities;

namespace ChronosTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SeriesName).HasMaxLength(200);
            entity.Property(e => e.FranchiseName).HasMaxLength(200);
        });
        modelBuilder.Entity<Game>().HasIndex(g => g.IGDBId).IsUnique();
    }
    }
