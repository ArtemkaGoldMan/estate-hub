using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Service interface for managing real estate listings
/// </summary>
public interface IListingService
{
    /// <summary>
    /// Gets all listings with optional filtering and pagination
    /// </summary>
    /// <param name="filter">Optional filter criteria for listings</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing listings</returns>
    Task<PagedResult<ListingDto>> GetAllAsync(ListingFilter? filter, int page, int pageSize);
    
    /// <summary>
    /// Gets a listing by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the listing</param>
    /// <returns>The listing if found, otherwise null</returns>
    Task<ListingDto?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all listings owned by the current user
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing user's listings</returns>
    Task<PagedResult<ListingDto>> GetMyAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets all listings liked by the current user
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing liked listings</returns>
    Task<PagedResult<ListingDto>> GetLikedAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets all archived listings for the current user
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing archived listings</returns>
    Task<PagedResult<ListingDto>> GetArchivedAsync(int page, int pageSize);
    
    /// <summary>
    /// Gets listings within specified geographic bounds
    /// </summary>
    /// <param name="bounds">Geographic bounds (latitude and longitude ranges)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional filter criteria for listings</param>
    /// <returns>Paged result containing listings within bounds</returns>
    Task<PagedResult<ListingDto>> GetWithinBoundsAsync(BoundsInput bounds, int page, int pageSize, ListingFilter? filter = null);
    
    /// <summary>
    /// Searches listings by text query
    /// </summary>
    /// <param name="text">Search text</param>
    /// <param name="filter">Optional filter criteria for listings</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paged result containing matching listings</returns>
    Task<PagedResult<ListingDto>> SearchAsync(string text, ListingFilter? filter, int page, int pageSize);
    
    /// <summary>
    /// Creates a new listing
    /// </summary>
    /// <param name="input">Listing creation input data</param>
    /// <returns>The unique identifier of the created listing</returns>
    Task<Guid> CreateAsync(CreateListingInput input);
    
    /// <summary>
    /// Updates an existing listing
    /// </summary>
    /// <param name="id">The unique identifier of the listing to update</param>
    /// <param name="input">Listing update input data</param>
    Task UpdateAsync(Guid id, UpdateListingInput input);
    
    /// <summary>
    /// Deletes a listing
    /// </summary>
    /// <param name="id">The unique identifier of the listing to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Changes the status of a listing
    /// </summary>
    /// <param name="id">The unique identifier of the listing</param>
    /// <param name="newStatus">The new status to set</param>
    Task ChangeStatusAsync(Guid id, ListingStatus newStatus);
    
    /// <summary>
    /// Admin operation to unpublish a listing
    /// </summary>
    /// <param name="id">The unique identifier of the listing</param>
    /// <param name="reason">Reason for unpublishing</param>
    Task AdminUnpublishAsync(Guid id, string reason);
    
    /// <summary>
    /// Adds a like to a listing for the current user
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing to like</param>
    Task LikeAsync(Guid listingId);
    
    /// <summary>
    /// Removes a like from a listing for the current user
    /// </summary>
    /// <param name="listingId">The unique identifier of the listing to unlike</param>
    Task UnlikeAsync(Guid listingId);
}

