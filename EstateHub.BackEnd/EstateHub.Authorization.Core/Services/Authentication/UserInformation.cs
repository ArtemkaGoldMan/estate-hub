using System.Security.Claims;
using EstateHub.Authorization.Core.Helpers;
using Newtonsoft.Json;

namespace EstateHub.Authorization.Core.Services.Authentication;

public class UserInformation
{
    public UserInformation(string userName, Guid userId, string role, Guid sessionId)
    {
        UserName = userName;
        UserId = userId;
        Role = role;
        SessionId = sessionId;
    }

    [JsonProperty(ClaimTypes.Name)] 
    public string UserName { get; init; }

    [JsonProperty(ClaimTypes.NameIdentifier)]
    public Guid UserId { get; init; }

    [JsonProperty(ClaimTypes.Role)] 
    public string Role { get; init; }
    
    [JsonProperty(JwtHelper.SessionIdClaimName)]
    public Guid SessionId { get; init; }
}