using GeoImageService.Application.Models.Images;
using Microsoft.EntityFrameworkCore;

namespace GeoImageService.Infrastructure.Contexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }
    
    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.GeoTiffFilePath).IsRequired();
            entity.OwnsOne(e => e.CornersCoordinates, corners =>
            {
                corners.OwnsOne(c => c.TopLeft);
                corners.OwnsOne(c => c.TopRight);
                corners.OwnsOne(c => c.BottomRight);
                corners.OwnsOne(c => c.BottomLeft);
            });
            entity.OwnsOne(e => e.TimeStamps);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }
}