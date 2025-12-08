namespace EstateHub.Authorization.API.Models.Requests;

/// <summary>
/// Request DTO for suspending a user account.
/// </summary>
public record SuspendUserRequest(string Reason);

