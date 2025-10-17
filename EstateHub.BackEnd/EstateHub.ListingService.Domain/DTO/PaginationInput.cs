namespace EstateHub.ListingService.Domain.DTO;

public record PaginationInput(
    int Page = 1,
    int PageSize = 20
);
