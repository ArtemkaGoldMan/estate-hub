using EstateHub.ListingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EstateHub.ListingService.DataAccess.SqlServer.Entities;

public class ListingEntity
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public ListingStatus Status { get; set; }
    public ListingCategory Category { get; set; }
    public PropertyType PropertyType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
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
    public decimal? PricePln { get; set; }
    public decimal? MonthlyRentPln { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Moderation fields
    public bool? IsModerationApproved { get; set; }
    public DateTime? ModerationCheckedAt { get; set; }
    public string? ModerationRejectionReason { get; set; }
    
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public List<ListingPhotoEntity> Photos { get; set; } = new();
    public List<LikedListingEntity> LikedByUsers { get; set; } = new();
}
