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

    [HttpGet("/user/{id:guid}")]
    public async Task<IResult> GetByIdAsync(Guid id, [FromQuery] bool includeDeleted = false)
    {
        return UserId == id
            ? await GetUserInfo<GetUserWithRolesResponse>(id, includeDeleted)
            : await GetUserInfo<GetUserResponse>(id, includeDeleted);
    }

    [HttpPost("/users/by-ids")]
    public async Task<IResult> GetByIdsAsync([FromForm] GetUsersByIdsRequest request, [FromQuery] bool includeDeleted = false)
    {
        var result = await _usersService.GetByIdsAsync<GetUserResponse>(request.Ids, includeDeleted);
        return result.IsSuccess
            ? Results.Ok(new GetUsersByIdsResponse { Users = result.Value, })
            : result.ToProblemDetails();
    }

    [HttpPatch("/user/{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UpdateByIdAsync(Guid id, [FromForm] UserUpdateRequest user)
    {
        if (UserId != id)
        {
            return Result.Failure(AuthorizationErrors.CanUpdateOnlySelf().ToString()).ToProblemDetails();
        }

        var result = await _usersService.UpdateByIdAsync(id, user);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }

    [HttpDelete("/user/{id:guid}")]
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
    [HttpGet("/admin/users")]
    [Authorize(Policy = "AdminAccess")]
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
