using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.DataAccess.SqlServer.Helpers;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _context;

    public ListingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Listing?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == id);

        return entity != null ? MapToDomain(entity) : null;
    }

    public async Task<IEnumerable<Listing>> GetAllAsync(int page, int pageSize, ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted)
            .AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        var entities = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<int> GetTotalCountAsync(ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted)
            .AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        return await query.GetCountAsync();
    }

    public async Task<IEnumerable<Listing>> GetByOwnerIdAsync(Guid ownerId)
    {
        var entities = await _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .Where(l => l.OwnerId == ownerId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task AddAsync(Listing listing)
    {
        var entity = MapToEntity(listing);
        _context.Listings.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Listing listing)
    {
        var entity = MapToEntity(listing);
        _context.Listings.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Listing>> GetWithinBoundsAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, int page, int pageSize, ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted &&
                       l.Latitude >= latMin && l.Latitude <= latMax &&
                       l.Longitude >= lonMin && l.Longitude <= lonMax)
            .AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        var entities = await query
            .OrderByDescending(l => l.CreatedAt)
            .ApplyPagination(page, pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<int> GetWithinBoundsCountAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted &&
                       l.Latitude >= latMin && l.Latitude <= latMax &&
                       l.Longitude >= lonMin && l.Longitude <= lonMax)
            .AsQueryable();

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        return await query.GetCountAsync();
    }

    public async Task<IEnumerable<Listing>> SearchAsync(string? text, int page, int pageSize, ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Include(l => l.Photos)
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(text))
        {
            query = query.Where(l => l.Title.Contains(text) || 
                                   l.Description.Contains(text) ||
                                   l.City.Contains(text) ||
                                   l.District.Contains(text) ||
                                   l.AddressLine.Contains(text));
        }

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        var entities = await query
            .OrderByDescending(l => l.CreatedAt)
            .ApplyPagination(page, pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task<int> SearchCountAsync(string? text, ListingFilter? filter = null)
    {
        var query = _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Published && !l.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(text))
        {
            query = query.Where(l => l.Title.Contains(text) || 
                                   l.Description.Contains(text) ||
                                   l.City.Contains(text) ||
                                   l.District.Contains(text) ||
                                   l.AddressLine.Contains(text));
        }

        if (filter != null)
        {
            query = ApplyFilter(query, filter);
        }

        return await query.GetCountAsync();
    }

    public async Task UpdateStatusAsync(Guid id, ListingStatus newStatus)
    {
        var entity = await _context.Listings.FindAsync(id);
        if (entity != null)
        {
            entity.Status = newStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            
            if (newStatus == ListingStatus.Published)
                entity.PublishedAt = DateTime.UtcNow;
            else if (newStatus == ListingStatus.Archived)
                entity.ArchivedAt = DateTime.UtcNow;
                
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Listings.FindAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
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
            Photos = photos,
            IsModerationApproved = entity.IsModerationApproved,
            ModerationCheckedAt = entity.ModerationCheckedAt,
            ModerationRejectionReason = entity.ModerationRejectionReason
        };
    }

    private static ListingEntity MapToEntity(Listing listing)
    {
        return new ListingEntity
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            Status = listing.Status,
            Category = listing.Category,
            PropertyType = listing.PropertyType,
            Title = listing.Title,
            Description = listing.Description,
            AddressLine = listing.AddressLine,
            District = listing.District,
            City = listing.City,
            PostalCode = listing.PostalCode,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            SquareMeters = listing.SquareMeters,
            Rooms = listing.Rooms,
            Floor = listing.Floor,
            FloorCount = listing.FloorCount,
            BuildYear = listing.BuildYear,
            Condition = listing.Condition,
            HasBalcony = listing.HasBalcony,
            HasElevator = listing.HasElevator,
            HasParkingSpace = listing.HasParkingSpace,
            HasSecurity = listing.HasSecurity,
            HasStorageRoom = listing.HasStorageRoom,
            PricePln = listing.PricePln,
            MonthlyRentPln = listing.MonthlyRentPln,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            PublishedAt = listing.PublishedAt,
            ArchivedAt = listing.ArchivedAt,
            IsDeleted = listing.IsDeleted,
            RowVersion = listing.RowVersion,
            IsModerationApproved = listing.IsModerationApproved,
            ModerationCheckedAt = listing.ModerationCheckedAt,
            ModerationRejectionReason = listing.ModerationRejectionReason,
            Photos = listing.Photos?.Select(p => new ListingPhotoEntity
            {
                Id = p.Id,
                ListingId = p.ListingId,
                Url = p.Url,
                Order = p.Order
            }).ToList() ?? new List<ListingPhotoEntity>()
        };
    }

    private static IQueryable<ListingEntity> ApplyFilter(IQueryable<ListingEntity> query, ListingFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(l => l.City.Contains(filter.City));

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(l => l.District.Contains(filter.District));

        if (filter.MinPrice.HasValue)
            query = query.Where(l => (l.PricePln ?? 0) >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => (l.PricePln ?? 0) <= filter.MaxPrice.Value);

        if (filter.MinMeters.HasValue)
            query = query.Where(l => l.SquareMeters >= filter.MinMeters.Value);

        if (filter.MaxMeters.HasValue)
            query = query.Where(l => l.SquareMeters <= filter.MaxMeters.Value);

        if (filter.MinRooms.HasValue)
            query = query.Where(l => l.Rooms >= filter.MinRooms.Value);

        if (filter.MaxRooms.HasValue)
            query = query.Where(l => l.Rooms <= filter.MaxRooms.Value);

        if (filter.HasElevator.HasValue)
            query = query.Where(l => l.HasElevator == filter.HasElevator.Value);

        if (filter.HasParkingSpace.HasValue)
            query = query.Where(l => l.HasParkingSpace == filter.HasParkingSpace.Value);

        if (filter.Category.HasValue)
            query = query.Where(l => l.Category == filter.Category.Value);

        if (filter.PropertyType.HasValue)
            query = query.Where(l => l.PropertyType == filter.PropertyType.Value);

        if (filter.Condition.HasValue)
            query = query.Where(l => l.Condition == filter.Condition.Value);

        if (filter.HasBalcony.HasValue)
            query = query.Where(l => l.HasBalcony == filter.HasBalcony.Value);

        if (filter.HasSecurity.HasValue)
            query = query.Where(l => l.HasSecurity == filter.HasSecurity.Value);

        if (filter.HasStorageRoom.HasValue)
            query = query.Where(l => l.HasStorageRoom == filter.HasStorageRoom.Value);

        return query;
    }
}
