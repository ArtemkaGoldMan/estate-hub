using EstateHub.Authorization.Domain.DTO.Session;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Domain.Interfaces.ApplicationInterfaces;
using EstateHub.SharedKernel.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateHub.Authorization.API.Controllers;

[Authorize]
public class SessionController : SessionAwareControllerBase
{
    private readonly ISessionsService _sessionService;

    public SessionController(
        ISessionsService sessionsService)
    {
        _sessionService = sessionsService;
    }

    [HttpGet("/session")]
    public async Task<IResult> GetByIdAsync()
    {
        var result = await _sessionService.GetByIdAsync<SessionDto>(SessionId);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
