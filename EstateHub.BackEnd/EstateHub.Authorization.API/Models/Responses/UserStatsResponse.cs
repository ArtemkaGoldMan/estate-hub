namespace EstateHub.Authorization.API.Models.Responses;

/// <summary>
/// Response DTO containing user statistics for admin dashboard.
/// </summary>
public record UserStatsResponse(
    int TotalUsers, 
    int ActiveUsers, 
    int SuspendedUsers, 
    int NewUsersThisMonth
);

