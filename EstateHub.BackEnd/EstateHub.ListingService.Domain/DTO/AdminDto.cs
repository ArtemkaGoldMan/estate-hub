namespace EstateHub.ListingService.Domain.DTO;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<string> Roles
);

public record UserStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int NewUsersThisMonth,
    int UsersWithListings
);

public record ListingStatsDto(
    int TotalListings,
    int PublishedListings,
    int PendingListings,
    int DraftListings,
    int NewListingsThisMonth,
    int TotalReports,
    int PendingReports
);

public record SystemStatsDto(
    UserStatsDto Users,
    ListingStatsDto Listings,
    DateTime GeneratedAt
);
