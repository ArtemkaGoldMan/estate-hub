using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.Interfaces;

public interface IListingService
{
    // Queries
    Task<PagedResult<ListingDto>> GetAllAsync(ListingFilter? filter, int page, int pageSize);
    Task<ListingDto?> GetByIdAsync(Guid id);
    Task<PagedResult<ListingDto>> GetMyAsync(int page, int pageSize);
    Task<PagedResult<ListingDto>> GetLikedAsync(int page, int pageSize);
    Task<PagedResult<ListingDto>> GetArchivedAsync(int page, int pageSize);
    Task<PagedResult<ListingDto>> GetWithinBoundsAsync(BoundsInput bounds, int page, int pageSize, ListingFilter? filter = null);
    Task<PagedResult<ListingDto>> SearchAsync(string text, ListingFilter? filter, int page, int pageSize);
    
    // Commands
    Task<Guid> CreateAsync(CreateListingInput input);
    Task UpdateAsync(Guid id, UpdateListingInput input);
    Task DeleteAsync(Guid id);
    Task ChangeStatusAsync(Guid id, ListingStatus newStatus);
    Task LikeAsync(Guid listingId);
    Task UnlikeAsync(Guid listingId);
}

