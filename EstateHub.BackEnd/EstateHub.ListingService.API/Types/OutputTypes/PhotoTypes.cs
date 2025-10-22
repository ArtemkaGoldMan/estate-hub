using EstateHub.ListingService.Domain.DTO;
using HotChocolate;

namespace EstateHub.ListingService.API.Types.OutputTypes;

public class PhotoType
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Order { get; set; }

    public static PhotoType FromDto(PhotoDto dto) => new()
    {
        Id = dto.Id,
        ListingId = dto.ListingId,
        Url = dto.Url,
        Order = dto.Order
    };
}
