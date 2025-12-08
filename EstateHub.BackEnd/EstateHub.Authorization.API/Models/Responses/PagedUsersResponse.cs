using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

namespace EstateHub.Authorization.API.Models.Responses;

/// <summary>
/// Response DTO for paginated user list.
/// </summary>
public record PagedUsersResponse(
    List<GetUserResponse> Users, 
    int Total, 
    int Page, 
    int PageSize
);

