using EstateHub.ListingService.Domain.DTO;

namespace EstateHub.ListingService.Core.Abstractions;

public interface IAdminService
{
    // User Management
    Task<PagedResult<UserDto>> GetUsersAsync(int page, int pageSize);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task AssignUserRoleAsync(Guid userId, string role);
    Task RemoveUserRoleAsync(Guid userId, string role);
    Task SuspendUserAsync(Guid userId, string reason);
    Task ActivateUserAsync(Guid userId);
    Task DeleteUserAsync(Guid userId);
    
    // Analytics
    Task<SystemStatsDto> GetSystemStatsAsync();
    Task<UserStatsDto> GetUserStatsAsync();
    Task<ListingStatsDto> GetListingStatsAsync();
}
