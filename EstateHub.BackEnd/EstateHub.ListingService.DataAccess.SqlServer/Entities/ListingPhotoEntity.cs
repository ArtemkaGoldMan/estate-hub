namespace EstateHub.ListingService.DataAccess.SqlServer.Entities;

public class ListingPhotoEntity
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }

    // Navigation properties
    public ListingEntity Listing { get; set; } = null!;
}
