using EstateHub.ListingService.Domain.Enums;

namespace EstateHub.ListingService.Domain.Models;

public record Listing
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public ListingStatus Status { get; init; }
    public ListingCategory Category { get; init; }
    public PropertyType PropertyType { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string AddressLine { get; init; }
    public string District { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal SquareMeters { get; init; }
    public int Rooms { get; init; }
    public int? Floor { get; init; }
    public int? FloorCount { get; init; }
    public int? BuildYear { get; init; }
    public Condition Condition { get; init; }
    public bool HasBalcony { get; init; }
    public bool HasElevator { get; init; }
    public bool HasParkingSpace { get; init; }
    public bool HasSecurity { get; init; }
    public bool HasStorageRoom { get; init; }
    public decimal? PricePln { get; init; }
    public decimal? MonthlyRentPln { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ArchivedAt { get; init; }
    public bool IsDeleted { get; init; }
    public byte[] RowVersion { get; init; }

    // Navigation properties
    public List<ListingPhoto> Photos { get; init; } = new();

    private Listing() { } // EF Core constructor

    public Listing(
        Guid ownerId,
        ListingCategory category,
        PropertyType propertyType,
        string title,
        string description,
        string addressLine,
        string district,
        string city,
        string postalCode,
        decimal latitude,
        decimal longitude,
        decimal squareMeters,
        int rooms,
        Condition condition,
        bool hasBalcony = false,
        bool hasElevator = false,
        bool hasParkingSpace = false,
        bool hasSecurity = false,
        bool hasStorageRoom = false,
        int? floor = null,
        int? floorCount = null,
        int? buildYear = null,
        decimal? pricePln = null,
        decimal? monthlyRentPln = null)
    {
        // Validation
        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId cannot be empty", nameof(ownerId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        if (string.IsNullOrWhiteSpace(addressLine))
            throw new ArgumentException("AddressLine cannot be null or empty", nameof(addressLine));

        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be null or empty", nameof(district));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("PostalCode cannot be null or empty", nameof(postalCode));

        if (squareMeters <= 0)
            throw new ArgumentException("SquareMeters must be positive", nameof(squareMeters));

        if (rooms < 1)
            throw new ArgumentException("Rooms must be at least 1", nameof(rooms));

        if (category == ListingCategory.Sale && pricePln is null)
            throw new ArgumentException("Sale listings require PricePln", nameof(pricePln));

        if (category == ListingCategory.Rent && monthlyRentPln is null)
            throw new ArgumentException("Rent listings require MonthlyRentPln", nameof(monthlyRentPln));

        if (floor.HasValue && floorCount.HasValue && floor.Value > floorCount.Value)
            throw new ArgumentException("Floor cannot be greater than FloorCount", nameof(floor));

        if (buildYear.HasValue && buildYear.Value > DateTime.Now.Year)
            throw new ArgumentException("BuildYear cannot be in the future", nameof(buildYear));

        if (pricePln.HasValue && pricePln.Value <= 0)
            throw new ArgumentException("PricePln must be positive", nameof(pricePln));

        if (monthlyRentPln.HasValue && monthlyRentPln.Value <= 0)
            throw new ArgumentException("MonthlyRentPln must be positive", nameof(monthlyRentPln));

        // Initialize properties
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Status = ListingStatus.Draft;
        Category = category;
        PropertyType = propertyType;
        Title = title;
        Description = description;
        AddressLine = addressLine;
        District = district;
        City = city;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        SquareMeters = squareMeters;
        Rooms = rooms;
        Floor = floor;
        FloorCount = floorCount;
        BuildYear = buildYear;
        Condition = condition;
        HasBalcony = hasBalcony;
        HasElevator = hasElevator;
        HasParkingSpace = hasParkingSpace;
        HasSecurity = hasSecurity;
        HasStorageRoom = hasStorageRoom;
        PricePln = pricePln;
        MonthlyRentPln = monthlyRentPln;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
        RowVersion = new byte[8]; // Will be set by EF Core
    }

    // Business methods - return new instances for immutability
    public Listing UpdateBasicInfo(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        return this with 
        { 
            Title = title, 
            Description = description, 
            UpdatedAt = DateTime.UtcNow 
        };
    }

    public Listing UpdateLocation(string addressLine, string district, string city, string postalCode, decimal latitude, decimal longitude)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
            throw new ArgumentException("AddressLine cannot be null or empty", nameof(addressLine));

        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be null or empty", nameof(district));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("PostalCode cannot be null or empty", nameof(postalCode));

        return this with
        {
            AddressLine = addressLine,
            District = district,
            City = city,
            PostalCode = postalCode,
            Latitude = latitude,
            Longitude = longitude,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing UpdatePropertyDetails(decimal squareMeters, int rooms, int? floor, int? floorCount, int? buildYear, Condition condition)
    {
        if (squareMeters <= 0)
            throw new ArgumentException("SquareMeters must be positive", nameof(squareMeters));

        if (rooms < 1)
            throw new ArgumentException("Rooms must be at least 1", nameof(rooms));

        if (floor.HasValue && floorCount.HasValue && floor.Value > floorCount.Value)
            throw new ArgumentException("Floor cannot be greater than FloorCount", nameof(floor));

        if (buildYear.HasValue && buildYear.Value > DateTime.Now.Year)
            throw new ArgumentException("BuildYear cannot be in the future", nameof(buildYear));

        return this with
        {
            SquareMeters = squareMeters,
            Rooms = rooms,
            Floor = floor,
            FloorCount = floorCount,
            BuildYear = buildYear,
            Condition = condition,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing UpdateAmenities(bool hasBalcony, bool hasElevator, bool hasParkingSpace, bool hasSecurity, bool hasStorageRoom)
    {
        return this with
        {
            HasBalcony = hasBalcony,
            HasElevator = hasElevator,
            HasParkingSpace = hasParkingSpace,
            HasSecurity = hasSecurity,
            HasStorageRoom = hasStorageRoom,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing UpdatePricing(decimal? pricePln, decimal? monthlyRentPln)
    {
        if (Category == ListingCategory.Sale && pricePln is null)
            throw new ArgumentException("Sale listings require PricePln", nameof(pricePln));

        if (Category == ListingCategory.Rent && monthlyRentPln is null)
            throw new ArgumentException("Rent listings require MonthlyRentPln", nameof(monthlyRentPln));

        if (pricePln.HasValue && pricePln.Value <= 0)
            throw new ArgumentException("PricePln must be positive", nameof(pricePln));

        if (monthlyRentPln.HasValue && monthlyRentPln.Value <= 0)
            throw new ArgumentException("MonthlyRentPln must be positive", nameof(monthlyRentPln));

        return this with
        {
            PricePln = pricePln,
            MonthlyRentPln = monthlyRentPln,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing Publish()
    {
        if (Status == ListingStatus.Published)
            throw new InvalidOperationException("Listing is already published");

        if (Status == ListingStatus.Archived)
            throw new InvalidOperationException("Cannot publish archived listing");

        return this with
        {
            Status = ListingStatus.Published,
            PublishedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing Archive()
    {
        if (Status == ListingStatus.Archived)
            throw new InvalidOperationException("Listing is already archived");

        return this with
        {
            Status = ListingStatus.Archived,
            ArchivedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing Unarchive()
    {
        if (Status != ListingStatus.Archived)
            throw new InvalidOperationException("Only archived listings can be unarchived");

        return this with
        {
            Status = ListingStatus.Draft,
            ArchivedAt = null,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing Delete()
    {
        return this with
        {
            IsDeleted = true,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Listing is not deleted");

        return this with
        {
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing AddPhoto(string url, int order)
    {
        var photo = new ListingPhoto(Id, url, order);
        var updatedPhotos = Photos.ToList();
        updatedPhotos.Add(photo);
        
        return this with
        {
            Photos = updatedPhotos,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing RemovePhoto(Guid photoId)
    {
        var updatedPhotos = Photos.Where(p => p.Id != photoId).ToList();
        
        return this with
        {
            Photos = updatedPhotos,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Listing ReorderPhotos(List<Guid> photoIdsInOrder)
    {
        if (photoIdsInOrder.Count != Photos.Count)
            throw new ArgumentException("Photo count mismatch", nameof(photoIdsInOrder));

        var reorderedPhotos = new List<ListingPhoto>();
        for (int i = 0; i < photoIdsInOrder.Count; i++)
        {
            var photo = Photos.FirstOrDefault(p => p.Id == photoIdsInOrder[i]);
            if (photo != null)
            {
                reorderedPhotos.Add(photo.UpdateOrder(i));
            }
        }

        return this with
        {
            Photos = reorderedPhotos,
            UpdatedAt = DateTime.UtcNow
        };
    }
}