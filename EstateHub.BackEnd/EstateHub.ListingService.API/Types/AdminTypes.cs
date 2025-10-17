using EstateHub.ListingService.Domain.DTO;
using HotChocolate;

namespace EstateHub.ListingService.API.Types;

public class UserType
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();

    public static UserType FromDto(UserDto dto) => new()
    {
        Id = dto.Id,
        Email = dto.Email,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        LastLoginAt = dto.LastLoginAt,
        Roles = dto.Roles
    };
}

public class PagedUsersType
{
    public List<UserType> Items { get; set; } = new();
    public int Total { get; set; }

    public static PagedUsersType FromDto(PagedResult<UserDto> dto) => new()
    {
        Items = dto.Items.Select(UserType.FromDto).ToList(),
        Total = dto.Total
    };
}

public class UserStatsType
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int UsersWithListings { get; set; }

    public static UserStatsType FromDto(UserStatsDto dto) => new()
    {
        TotalUsers = dto.TotalUsers,
        ActiveUsers = dto.ActiveUsers,
        NewUsersThisMonth = dto.NewUsersThisMonth,
        UsersWithListings = dto.UsersWithListings
    };
}

public class ListingStatsType
{
    public int TotalListings { get; set; }
    public int PublishedListings { get; set; }
    public int PendingListings { get; set; }
    public int DraftListings { get; set; }
    public int NewListingsThisMonth { get; set; }
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }

    public static ListingStatsType FromDto(ListingStatsDto dto) => new()
    {
        TotalListings = dto.TotalListings,
        PublishedListings = dto.PublishedListings,
        PendingListings = dto.PendingListings,
        DraftListings = dto.DraftListings,
        NewListingsThisMonth = dto.NewListingsThisMonth,
        TotalReports = dto.TotalReports,
        PendingReports = dto.PendingReports
    };
}

public class SystemStatsType
{
    public UserStatsType Users { get; set; } = new();
    public ListingStatsType Listings { get; set; } = new();
    public DateTime GeneratedAt { get; set; }

    public static SystemStatsType FromDto(SystemStatsDto dto) => new()
    {
        Users = UserStatsType.FromDto(dto.Users),
        Listings = ListingStatsType.FromDto(dto.Listings),
        GeneratedAt = dto.GeneratedAt
    };
}
