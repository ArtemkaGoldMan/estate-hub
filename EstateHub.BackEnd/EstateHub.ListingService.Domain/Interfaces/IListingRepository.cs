using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(Guid id);
    Task<IEnumerable<Listing>> GetAllAsync(int page, int pageSize, ListingFilter? filter = null);
    Task<int> GetTotalCountAsync(ListingFilter? filter = null);
    Task<IEnumerable<Listing>> GetByOwnerIdAsync(Guid ownerId);
    Task<IEnumerable<Listing>> GetWithinBoundsAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, int page, int pageSize, ListingFilter? filter = null);
    Task<int> GetWithinBoundsCountAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, ListingFilter? filter = null);
    Task<IEnumerable<Listing>> SearchAsync(string? text, int page, int pageSize, ListingFilter? filter = null);
    Task<int> SearchCountAsync(string? text, ListingFilter? filter = null);
    Task AddAsync(Listing listing);
    Task UpdateAsync(Listing listing);
    Task UpdateStatusAsync(Guid id, ListingStatus newStatus);
    Task DeleteAsync(Guid id);
}
