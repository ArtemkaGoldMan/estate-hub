using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Db;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ListingEntity> Listings { get; set; }
    public DbSet<ListingPhotoEntity> ListingPhotos { get; set; }
    public DbSet<LikedListingEntity> LikedListings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Listings
        modelBuilder.Entity<ListingEntity>(entity =>
        {
            entity.ToTable("Listings");
            
            entity.HasKey(l => l.Id);
            
            entity.Property(l => l.Id).ValueGeneratedNever();
            entity.Property(l => l.Title).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Description).IsRequired().HasMaxLength(2000);
            entity.Property(l => l.AddressLine).IsRequired().HasMaxLength(200);
            entity.Property(l => l.District).IsRequired().HasMaxLength(100);
            entity.Property(l => l.City).IsRequired().HasMaxLength(100);
            entity.Property(l => l.PostalCode).IsRequired().HasMaxLength(10);
            entity.Property(l => l.Latitude).HasColumnType("decimal(10,7)").IsRequired();
            entity.Property(l => l.Longitude).HasColumnType("decimal(10,7)").IsRequired();
            entity.Property(l => l.SquareMeters).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(l => l.PricePln).HasColumnType("decimal(15,2)");
            entity.Property(l => l.MonthlyRentPln).HasColumnType("decimal(15,2)");
            entity.Property(l => l.RowVersion).IsRowVersion();
            
            // Soft delete filter
            entity.HasQueryFilter(l => !l.IsDeleted);
            
            // Indexes
            entity.HasIndex(l => l.OwnerId);
            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.Category);
            entity.HasIndex(l => l.City);
            entity.HasIndex(l => l.District);
            entity.HasIndex(l => new { l.City, l.District, l.Status });
            
            // Relationships
            entity.HasMany(l => l.Photos)
                .WithOne(p => p.Listing)
                .HasForeignKey(p => p.ListingId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
                
            entity.HasMany(l => l.LikedByUsers)
                .WithOne(l => l.Listing)
                .HasForeignKey(l => l.ListingId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        // Configure ListingPhotos
        modelBuilder.Entity<ListingPhotoEntity>(entity =>
        {
            entity.ToTable("ListingPhotos");
            
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Url).IsRequired().HasMaxLength(500);
            
            // Indexes
            entity.HasIndex(p => p.ListingId);
            entity.HasIndex(p => new { p.ListingId, p.Order });
        });

        // Configure LikedListings
        modelBuilder.Entity<LikedListingEntity>(entity =>
        {
            entity.ToTable("LikedListings");
            
            entity.HasKey(l => new { l.UserId, l.ListingId });
            
            // Indexes
            entity.HasIndex(l => l.UserId);
            entity.HasIndex(l => l.ListingId);
            entity.HasIndex(l => l.CreatedAt);
        });
    }
}
