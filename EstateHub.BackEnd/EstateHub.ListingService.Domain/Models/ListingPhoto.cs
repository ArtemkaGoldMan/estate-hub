namespace EstateHub.ListingService.Domain.Models;

public record ListingPhoto
{
    public Guid Id { get; init; }
    public Guid ListingId { get; init; }
    public string Url { get; init; }
    public int Order { get; init; }

    private ListingPhoto() { } // EF Core constructor

    public ListingPhoto(Guid listingId, string url, int order)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Photo URL cannot be null or empty", nameof(url));
        
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        Id = Guid.NewGuid();
        ListingId = listingId;
        Url = url;
        Order = order;
    }

    public ListingPhoto UpdateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Photo URL cannot be null or empty", nameof(url));
        
        return this with { Url = url };
    }

    public ListingPhoto UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));
        
        return this with { Order = order };
    }
}