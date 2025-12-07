using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.DTO.Authentication.Requests;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.Authorization.Domain.Interfaces.CoreInterfaces;
using EstateHub.SharedKernel.API.Extensions;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace EstateHub.Authorization.API.Controllers;

[Authorize]
public class UserController : SessionAwareControllerBase
{
    private readonly IUsersService _usersService;

    public UserController(
        IUsersService usersService)
    {
        _usersService = usersService;
    }

    /// <summary>
    /// Gets user information by ID.
    /// Returns user with roles if requesting own profile, otherwise returns basic user info.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users.</param>
    /// <returns>User information.</returns>
    /// <response code="200">User found. Returns user information.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("/user/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAsync(Guid id, [FromQuery] bool includeDeleted = false)
    {
        return UserId == id
            ? await GetUserInfo<GetUserWithRolesResponse>(id, includeDeleted)
            : await GetUserInfo<GetUserResponse>(id, includeDeleted);
    }

    /// <summary>
    /// Gets multiple users by their IDs (batch lookup).
    /// </summary>
    /// <param name="request">Request containing list of user IDs.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users.</param>
    /// <returns>List of user information.</returns>
    /// <response code="200">Returns list of users.</response>
    /// <response code="400">Request failed due to invalid input.</response>
    [HttpPost("/users/by-ids")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> GetByIdsAsync([FromForm] GetUsersByIdsRequest request, [FromQuery] bool includeDeleted = false)
    {
        var result = await _usersService.GetByIdsAsync<GetUserResponse>(request.Ids, includeDeleted);
        return result.IsSuccess
            ? Results.Ok(new GetUsersByIdsResponse { Users = result.Value, })
            : result.ToProblemDetails();
    }

    /// <summary>
    /// Updates user profile information. Users can only update their own profile.
    /// </summary>
    /// <param name="id">The user ID to update (must match authenticated user).</param>
    /// <param name="user">User update request containing fields to update.</param>
    /// <returns>Empty response if update succeeds.</returns>
    /// <response code="204">User updated successfully.</response>
    /// <response code="400">Update failed due to validation errors.</response>
    /// <response code="403">User attempted to update another user's profile.</response>
    [HttpPatch("/user/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> UpdateByIdAsync(Guid id, [FromForm] UserUpdateRequest user)
    {
        if (UserId != id)
        {
            return Result.Failure(AuthorizationErrors.CanUpdateOnlySelf().ToString()).ToProblemDetails();
        }

        var result = await _usersService.UpdateByIdAsync(id, user);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    /// <summary>
    /// Soft-deletes user account. Users can only delete their own account.
    /// </summary>
    /// <param name="id">The user ID to delete (must match authenticated user).</param>
    /// <returns>Empty response if deletion succeeds.</returns>
    /// <response code="204">User deleted successfully.</response>
    /// <response code="403">User attempted to delete another user's account.</response>
    /// <response code="404">User not found.</response>
    [HttpDelete("/user/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteByIdAsync(Guid id)
    {
        if (UserId != id)
        {
            return Result.Failure(AuthorizationErrors.CanDeleteOnlySelf().ToString()).ToProblemDetails();
        }

        var result = await _usersService.DeleteByIdAsync(id);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    // Admin endpoints
    
    /// <summary>
    /// Gets paginated list of all users. Admin only.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted users.</param>
    /// <returns>Paginated list of users.</returns>
    /// <response code="200">Returns paginated user list.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have admin access.</response>
    [HttpGet("/admin/users")]
    [Authorize(Policy = "AdminAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> GetUsersAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeDeleted = false)
    {
        var result = await _usersService.GetUsersAsync<GetUserResponse>(page, pageSize, includeDeleted);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("/admin/users/stats")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> GetUserStatsAsync()
    {
        var result = await _usersService.GetUserStatsAsync();
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("/admin/users/{id:guid}/roles")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> AssignUserRoleAsync(Guid id, [FromBody] AssignRoleRequest request)
    {
        var result = await _usersService.AssignUserRoleAsync(id, request.Role);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("/admin/users/{id:guid}/roles/{role}")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> RemoveUserRoleAsync(Guid id, string role)
    {
        var result = await _usersService.RemoveUserRoleAsync(id, role, UserId);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    [HttpPost("/admin/users/{id:guid}/suspend")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> SuspendUserAsync(Guid id, [FromBody] SuspendUserRequest request)
    {
        var result = await _usersService.SuspendUserAsync(id, request.Reason);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    [HttpPost("/admin/users/{id:guid}/activate")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> ActivateUserAsync(Guid id)
    {
        var result = await _usersService.ActivateUserAsync(id);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("/admin/users/{id:guid}")]
    [Authorize(Policy = "AdminAccess")]
    public async Task<IResult> AdminDeleteUserAsync(Guid id)
    {
        var result = await _usersService.AdminDeleteUserAsync(id);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    private async Task<IResult> GetUserInfo<T>(Guid id, bool includeDeleted)
        where T : class
    {
        var result = await _usersService.GetByIdAsync<T>(id, includeDeleted);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}

// Request/Response DTOs for admin endpoints
public record AssignRoleRequest(string Role);
public record SuspendUserRequest(string Reason);
public record UserStatsResponse(int TotalUsers, int ActiveUsers, int SuspendedUsers, int NewUsersThisMonth);
public record PagedUsersResponse(List<GetUserResponse> Users, int Total, int Page, int PageSize);
