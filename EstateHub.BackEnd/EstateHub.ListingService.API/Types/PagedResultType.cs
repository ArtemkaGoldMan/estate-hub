using EstateHub.ListingService.Domain.DTO;
using HotChocolate;

namespace EstateHub.ListingService.API.Types;

public class PagedListingsType
{
    public List<ListingType> Items { get; set; } = new();
    public int Total { get; set; }

    public static PagedListingsType FromDto(PagedResult<ListingDto> dto) => new()
    {
        Items = dto.Items.Select(ListingType.FromDto).ToList(),
        Total = dto.Total
    };
}
