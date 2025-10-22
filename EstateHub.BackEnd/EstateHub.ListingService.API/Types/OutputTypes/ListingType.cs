using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate;

namespace EstateHub.ListingService.API.Types.OutputTypes;

public class ListingType
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? PricePln { get; set; }
    public decimal? MonthlyRentPln { get; set; }
    public ListingStatus Status { get; set; }
    public ListingCategory Category { get; set; }
    public PropertyType PropertyType { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal SquareMeters { get; set; }
    public int Rooms { get; set; }
    public int? Floor { get; set; }
    public int? FloorCount { get; set; }
    public int? BuildYear { get; set; }
    public Condition Condition { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasElevator { get; set; }
    public bool HasParkingSpace { get; set; }
    public bool HasSecurity { get; set; }
    public bool HasStorageRoom { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? FirstPhotoUrl { get; set; }
    public bool IsLikedByCurrentUser { get; set; }

    public static ListingType FromDto(ListingDto dto) => new()
    {
        Id = dto.Id,
        OwnerId = dto.OwnerId,
        Title = dto.Title,
        Description = dto.Description,
        PricePln = dto.PricePln,
        MonthlyRentPln = dto.MonthlyRentPln,
        Status = dto.Status,
        Category = dto.Category,
        PropertyType = dto.PropertyType,
        City = dto.City,
        District = dto.District,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
        SquareMeters = dto.SquareMeters,
        Rooms = dto.Rooms,
        Floor = dto.Floor,
        FloorCount = dto.FloorCount,
        BuildYear = dto.BuildYear,
        Condition = dto.Condition,
        HasBalcony = dto.HasBalcony,
        HasElevator = dto.HasElevator,
        HasParkingSpace = dto.HasParkingSpace,
        HasSecurity = dto.HasSecurity,
        HasStorageRoom = dto.HasStorageRoom,
        CreatedAt = dto.CreatedAt,
        UpdatedAt = dto.UpdatedAt,
        PublishedAt = dto.PublishedAt,
        ArchivedAt = dto.ArchivedAt,
        FirstPhotoUrl = dto.FirstPhotoUrl,
        IsLikedByCurrentUser = dto.IsLikedByCurrentUser
    };
}
