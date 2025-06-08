using System.Net.Mime;
using System.Security.Claims;
using CSharpFunctionalExtensions;
using EstateHub.Authorization.Domain.Errors;
using EstateHub.Authorization.Core.Helpers;
using EstateHub.SharedKernel.API;
using Microsoft.AspNetCore.Mvc;

namespace EstateHub.Authorization.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class SessionAwareControllerBase : AuthorizedControllerBase
{
    protected Guid SessionId
    {
        get
        {
            var claim = HttpContext.User
                .Claims
                .FirstOrDefault(x => x.Type == JwtHelper.SessionIdClaimName);

            if (claim is null)
            {
                throw new Exception("SessionId claim is null");
            }

            var success = Guid.TryParse(claim.Value, out var sessionId);
            if (!success)
            {
                throw new Exception(AuthorizationErrors.InvalidAccessToken().ToString());
            }

            return sessionId;
        }
    }
}
