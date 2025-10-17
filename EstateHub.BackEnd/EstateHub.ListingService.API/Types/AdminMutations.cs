using EstateHub.ListingService.Core.Abstractions;
using EstateHub.SharedKernel.API.Authorization.Attributes;
using HotChocolate;
using HotChocolate.Authorization;

namespace EstateHub.ListingService.API.Types;

public class AdminMutations
{
    [Authorize]
    [RequirePermission("RoleManagement")]
    public async Task<bool> AssignUserRole(
        Guid userId,
        string role,
        [Service] IAdminService adminService)
    {
        await adminService.AssignUserRoleAsync(userId, role);
        return true;
    }

    [Authorize]
    [RequirePermission("RoleManagement")]
    public async Task<bool> RemoveUserRole(
        Guid userId,
        string role,
        [Service] IAdminService adminService)
    {
        await adminService.RemoveUserRoleAsync(userId, role);
        return true;
    }

    [Authorize]
    [RequirePermission("UserManagement")]
    public async Task<bool> SuspendUser(
        Guid userId,
        string reason,
        [Service] IAdminService adminService)
    {
        await adminService.SuspendUserAsync(userId, reason);
        return true;
    }

    [Authorize]
    [RequirePermission("UserManagement")]
    public async Task<bool> ActivateUser(
        Guid userId,
        [Service] IAdminService adminService)
    {
        await adminService.ActivateUserAsync(userId);
        return true;
    }

    [Authorize]
    [RequirePermission("UserManagement")]
    public async Task<bool> DeleteUser(
        Guid userId,
        [Service] IAdminService adminService)
    {
        await adminService.DeleteUserAsync(userId);
        return true;
    }
}
