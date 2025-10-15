namespace EstateHub.ListingService.DataAccess.SqlServer.Entities;

public class LikedListingEntity
{
    public Guid UserId { get; set; }
    public Guid ListingId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ListingEntity Listing { get; set; } = null!;
}
