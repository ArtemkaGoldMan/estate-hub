using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class LikedListingRepository : ILikedListingRepository
{
    private readonly ApplicationDbContext _context;

    public LikedListingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Listing>> GetLikedByUserAsync(Guid userId)
    {
        var listingIds = await _context.LikedListings
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => l.ListingId)
            .ToListAsync();

        var entities = await _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .Where(l => listingIds.Contains(l.Id))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task LikeAsync(Guid userId, Guid listingId)
    {
        var existingLike = await _context.LikedListings
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListingId == listingId);

        if (existingLike == null)
        {
            var likedListing = new LikedListingEntity
            {
                UserId = userId,
                ListingId = listingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LikedListings.Add(likedListing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnlikeAsync(Guid userId, Guid listingId)
    {
        var likedListing = await _context.LikedListings
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ListingId == listingId);

        if (likedListing != null)
        {
            _context.LikedListings.Remove(likedListing);
            await _context.SaveChangesAsync();
        }
    }

    private static Listing MapToDomain(ListingEntity entity)
    {
        var photos = entity.Photos?.Select(p => new ListingPhoto(p.ListingId, p.Url, p.Order) { Id = p.Id }).ToList() ?? new List<ListingPhoto>();

        return new Listing(
            entity.OwnerId,
            entity.Category,
            entity.PropertyType,
            entity.Title,
            entity.Description,
            entity.AddressLine,
            entity.District,
            entity.City,
            entity.PostalCode,
            entity.Latitude,
            entity.Longitude,
            entity.SquareMeters,
            entity.Rooms,
            entity.Condition,
            entity.HasBalcony,
            entity.HasElevator,
            entity.HasParkingSpace,
            entity.HasSecurity,
            entity.HasStorageRoom,
            entity.Floor,
            entity.FloorCount,
            entity.BuildYear,
            entity.PricePln,
            entity.MonthlyRentPln)
        {
            Id = entity.Id,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            PublishedAt = entity.PublishedAt,
            ArchivedAt = entity.ArchivedAt,
            IsDeleted = entity.IsDeleted,
            RowVersion = entity.RowVersion,
            Photos = photos
        };
    }
}
