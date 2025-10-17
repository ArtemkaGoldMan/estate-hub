using EstateHub.ListingService.Domain.DTO;
using HotChocolate;

namespace EstateHub.ListingService.API.Types;

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

public class AddPhotoInputType
{
    public Guid ListingId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}

public class ReorderPhotosInputType
{
    public Guid ListingId { get; set; }
    public List<Guid> OrderedPhotoIds { get; set; } = new();
}
