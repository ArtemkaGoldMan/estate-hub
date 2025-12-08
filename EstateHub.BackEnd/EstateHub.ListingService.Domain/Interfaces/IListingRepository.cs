using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using EstateHub.ListingService.Domain.Models;

namespace EstateHub.ListingService.Domain.Interfaces;

/// <summary>
/// Repository interface for listing data access operations
/// </summary>
public interface IListingRepository
{
    /// <summary>
    /// Gets a listing by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the listing</param>
    /// <returns>The listing if found, otherwise null</returns>
    Task<Listing?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all listings with pagination and optional filtering
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Collection of listings</returns>
    Task<IEnumerable<Listing>> GetAllAsync(int page, int pageSize, ListingFilter? filter = null);
    
    /// <summary>
    /// Gets the total count of listings matching optional filter
    /// </summary>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Total count of listings</returns>
    Task<int> GetTotalCountAsync(ListingFilter? filter = null);
    
    /// <summary>
    /// Gets all listings owned by a specific user
    /// </summary>
    /// <param name="ownerId">The unique identifier of the owner</param>
    /// <returns>Collection of listings owned by the user</returns>
    Task<IEnumerable<Listing>> GetByOwnerIdAsync(Guid ownerId);
    
    /// <summary>
    /// Gets listings within specified geographic bounds
    /// </summary>
    /// <param name="latMin">Minimum latitude</param>
    /// <param name="latMax">Maximum latitude</param>
    /// <param name="lonMin">Minimum longitude</param>
    /// <param name="lonMax">Maximum longitude</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Collection of listings within bounds</returns>
    Task<IEnumerable<Listing>> GetWithinBoundsAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, int page, int pageSize, ListingFilter? filter = null);
    
    /// <summary>
    /// Gets the count of listings within specified geographic bounds
    /// </summary>
    /// <param name="latMin">Minimum latitude</param>
    /// <param name="latMax">Maximum latitude</param>
    /// <param name="lonMin">Minimum longitude</param>
    /// <param name="lonMax">Maximum longitude</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Count of listings within bounds</returns>
    Task<int> GetWithinBoundsCountAsync(decimal latMin, decimal latMax, decimal lonMin, decimal lonMax, ListingFilter? filter = null);
    
    /// <summary>
    /// Searches listings by text query
    /// </summary>
    /// <param name="text">Search text</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Collection of matching listings</returns>
    Task<IEnumerable<Listing>> SearchAsync(string? text, int page, int pageSize, ListingFilter? filter = null);
    
    /// <summary>
    /// Gets the count of listings matching search text
    /// </summary>
    /// <param name="text">Search text</param>
    /// <param name="filter">Optional filter criteria</param>
    /// <returns>Count of matching listings</returns>
    Task<int> SearchCountAsync(string? text, ListingFilter? filter = null);
    
    /// <summary>
    /// Adds a new listing to the repository
    /// </summary>
    /// <param name="listing">The listing entity to add</param>
    Task AddAsync(Listing listing);
    
    /// <summary>
    /// Updates an existing listing in the repository
    /// </summary>
    /// <param name="listing">The listing entity with updated data</param>
    Task UpdateAsync(Listing listing);
    
    /// <summary>
    /// Updates only the status of a listing
    /// </summary>
    /// <param name="id">The unique identifier of the listing</param>
    /// <param name="newStatus">The new status to set</param>
    Task UpdateStatusAsync(Guid id, ListingStatus newStatus);
    
    /// <summary>
    /// Deletes a listing from the repository
    /// </summary>
    /// <param name="id">The unique identifier of the listing to delete</param>
    Task DeleteAsync(Guid id);
}
