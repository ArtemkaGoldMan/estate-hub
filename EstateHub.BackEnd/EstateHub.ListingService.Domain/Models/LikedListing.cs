namespace EstateHub.ListingService.Domain.Models;

public record LikedListing
{
    public Guid UserId { get; init; }
    public Guid ListingId { get; init; }
    public DateTime CreatedAt { get; init; }

    private LikedListing() { } // EF Core constructor

    public LikedListing(Guid userId, Guid listingId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId cannot be empty", nameof(listingId));

        UserId = userId;
        ListingId = listingId;
        CreatedAt = DateTime.UtcNow;
    }
}