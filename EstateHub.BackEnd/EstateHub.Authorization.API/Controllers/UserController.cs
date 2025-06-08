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

    private async Task<IResult> GetUserInfo<T>(Guid id, bool includeDeleted)
        where T : class
    {
        var result = await _usersService.GetByIdAsync<T>(id, includeDeleted);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
