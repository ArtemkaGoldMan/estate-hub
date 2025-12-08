using EstateHub.ListingService.Domain.DTO;
using HotChocolate;

namespace EstateHub.ListingService.API.Types.OutputTypes;

public class ModerationResultType
{
    public bool IsApproved { get; set; }
    
    [GraphQLDescription("Reason for rejection, if not approved")]
    public string? RejectionReason { get; set; }
    
    [GraphQLDescription("Suggestions for improving the content")]
    public List<string>? Suggestions { get; set; }

    public static ModerationResultType FromDto(ModerationResult dto) => new()
    {
        IsApproved = dto.IsApproved,
        RejectionReason = dto.RejectionReason,
        Suggestions = dto.Suggestions
    };
}









