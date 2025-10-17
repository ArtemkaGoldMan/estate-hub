namespace EstateHub.ListingService.Domain.DTO;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Total,
    int Page,
    int PageSize
);
