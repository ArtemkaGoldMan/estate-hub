using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EstateHub.SharedKernel.API;

[Microsoft.AspNetCore.Components.Route("api/[controller]")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public abstract class AuthorizedControllerBase : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            var claim = HttpContext.User
                .Claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (claim is null)
            {
                throw new Exception("UserId claim is null");
            }

            var success = Guid.TryParse(claim.Value, out var userId);
            if (!success)
            {
                throw new Exception("Invalid UserId claim format");
            }

            return userId;
        }
    }
}
