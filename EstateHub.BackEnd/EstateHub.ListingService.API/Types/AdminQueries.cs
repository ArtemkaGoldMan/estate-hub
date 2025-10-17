using EstateHub.ListingService.Core.Abstractions;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types;

public class AdminQueries
{
    [Authorize]
    [RequirePermission("UserManagement")]
    public async Task<PagedUsersType> GetUsers(
        int page,
        int pageSize,
        [Service] IAdminService adminService)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);
        
        var result = await adminService.GetUsersAsync(page, pageSize);
        return PagedUsersType.FromDto(result);
    }

    [Authorize]
    [RequirePermission("UserManagement")]
    public async Task<UserType?> GetUser(
        Guid userId,
        [Service] IAdminService adminService)
    {
        var result = await adminService.GetUserByIdAsync(userId);
        return result != null ? UserType.FromDto(result) : null;
    }

    [Authorize]
    [RequirePermission("ViewAnalytics")]
    public async Task<SystemStatsType> GetSystemStats(
        [Service] IAdminService adminService)
    {
        var result = await adminService.GetSystemStatsAsync();
        return SystemStatsType.FromDto(result);
    }

    [Authorize]
    [RequirePermission("ViewAnalytics")]
    public async Task<UserStatsType> GetUserStats(
        [Service] IAdminService adminService)
    {
        var result = await adminService.GetUserStatsAsync();
        return UserStatsType.FromDto(result);
    }

    [Authorize]
    [RequirePermission("ViewAnalytics")]
    public async Task<ListingStatsType> GetListingStats(
        [Service] IAdminService adminService)
    {
        var result = await adminService.GetListingStatsAsync();
        return ListingStatsType.FromDto(result);
    }
}
