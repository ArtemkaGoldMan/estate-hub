using EstateHub.ListingService.Core.DTO;
using EstateHub.ListingService.Domain.DTO;
using EstateHub.ListingService.Domain.Enums;
using HotChocolate;

namespace EstateHub.ListingService.API.Types;

public class CreateListingInputType
{
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
    public ListingCategory Category { get; set; }
    public PropertyType PropertyType { get; set; }

    public Core.DTO.CreateListingInput ToDto() => new(
        Category,
        PropertyType,
        Title,
        Description,
        AddressLine,
        District,
        City,
        PostalCode,
        Latitude,
        Longitude,
        SquareMeters,
        Rooms,
        Floor,
        FloorCount,
        BuildYear,
        Condition,
        HasBalcony,
        HasElevator,
        HasParkingSpace,
        HasSecurity,
        HasStorageRoom,
        PricePln,
        MonthlyRentPln
    );
}

public class UpdateListingInputType
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AddressLine { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? SquareMeters { get; set; }
    public int? Rooms { get; set; }
    public int? Floor { get; set; }
    public int? FloorCount { get; set; }
    public int? BuildYear { get; set; }
    public Condition? Condition { get; set; }
    public bool? HasBalcony { get; set; }
    public bool? HasElevator { get; set; }
    public bool? HasParkingSpace { get; set; }
    public bool? HasSecurity { get; set; }
    public bool? HasStorageRoom { get; set; }
    public decimal? PricePln { get; set; }
    public decimal? MonthlyRentPln { get; set; }

    public Core.DTO.UpdateListingInput ToDto() => new(
        Title,
        Description,
        AddressLine,
        District,
        City,
        PostalCode,
        Latitude,
        Longitude,
        SquareMeters,
        Rooms,
        Floor,
        FloorCount,
        BuildYear,
        Condition,
        HasBalcony,
        HasElevator,
        HasParkingSpace,
        HasSecurity,
        HasStorageRoom,
        PricePln,
        MonthlyRentPln
    );
}

public class ChangeStatusInputType
{
    public ListingStatus NewStatus { get; set; }

    public Core.DTO.ChangeStatusInput ToDto() => new(NewStatus);
}

public class PaginationInputType
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public Core.DTO.PaginationInput ToDto() => new(Page, PageSize);
}

public class BoundsInputType
{
    public decimal LatMin { get; set; }
    public decimal LatMax { get; set; }
    public decimal LonMin { get; set; }
    public decimal LonMax { get; set; }

    public Core.DTO.BoundsInput ToDto() => new(LatMin, LatMax, LonMin, LonMax);
}

public class ListingFilterType
{
    public string? City { get; set; }
    public string? District { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinMeters { get; set; }
    public decimal? MaxMeters { get; set; }
    public int? MinRooms { get; set; }
    public int? MaxRooms { get; set; }
    public bool? HasElevator { get; set; }
    public bool? HasParkingSpace { get; set; }
    public ListingCategory? Category { get; set; }
    public PropertyType? PropertyType { get; set; }
    public Condition? Condition { get; set; }
    public bool? HasBalcony { get; set; }
    public bool? HasSecurity { get; set; }
    public bool? HasStorageRoom { get; set; }

    public Domain.DTO.ListingFilter ToDto() => new(
        City,
        District,
        MinPrice,
        MaxPrice,
        MinMeters,
        MaxMeters,
        MinRooms,
        MaxRooms,
        HasElevator,
        HasParkingSpace,
        Category,
        PropertyType,
        Condition,
        HasBalcony,
        HasSecurity,
        HasStorageRoom
    );
}