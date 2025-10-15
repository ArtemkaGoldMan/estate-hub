using EstateHub.ListingService.DataAccess.SqlServer.Db;
using EstateHub.ListingService.DataAccess.SqlServer.Entities;
using EstateHub.ListingService.Domain.Interfaces;
using EstateHub.ListingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Repositories;

public class PhotoRepository : IPhotoRepository
{
    private readonly ApplicationDbContext _context;

    public PhotoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ListingPhoto> AddPhotoAsync(Guid listingId, string url)
    {
        // Get the next order number for this listing
        var maxOrder = await _context.ListingPhotos
            .Where(p => p.ListingId == listingId)
            .MaxAsync(p => (int?)p.Order) ?? -1;

        var photoEntity = new ListingPhotoEntity
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            Url = url,
            Order = maxOrder + 1
        };

        _context.ListingPhotos.Add(photoEntity);
        await _context.SaveChangesAsync();

        return new ListingPhoto(listingId, url, photoEntity.Order) { Id = photoEntity.Id };
    }

    public async Task RemovePhotoAsync(Guid listingId, Guid photoId)
    {
        var photo = await _context.ListingPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.ListingId == listingId);

        if (photo != null)
        {
            _context.ListingPhotos.Remove(photo);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReorderPhotosAsync(Guid listingId, IEnumerable<Guid> orderedIds)
    {
        var photoIds = orderedIds.ToList();
        var photos = await _context.ListingPhotos
            .Where(p => p.ListingId == listingId && photoIds.Contains(p.Id))
            .ToListAsync();

        for (int i = 0; i < photoIds.Count; i++)
        {
            var photo = photos.FirstOrDefault(p => p.Id == photoIds[i]);
            if (photo != null)
            {
                photo.Order = i;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ListingPhoto>> GetPhotosByListingIdAsync(Guid listingId)
    {
        var entities = await _context.ListingPhotos
            .AsNoTracking()
            .Where(p => p.ListingId == listingId)
            .OrderBy(p => p.Order)
            .ToListAsync();

        return entities.Select(p => new ListingPhoto(p.ListingId, p.Url, p.Order) { Id = p.Id });
    }
}
