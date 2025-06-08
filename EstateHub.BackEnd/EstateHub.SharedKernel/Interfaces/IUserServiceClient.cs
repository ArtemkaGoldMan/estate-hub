using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Requests;
using EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

namespace EstateHub.SharedKernel.API.Interfaces;

public interface IUserServiceClient
{
    Task<UserIdFromTokenResponse?> GetUserIdFromTokenAsync();
    Task<GetUserResponse?> GetUserByIdAsync(Guid id, bool includeDeleted = false);
    Task<GetUsersByIdsResponse?> GetUsersByIdsAsync(GetUsersByIdsRequest getUsersByIdsRequest, bool includeDeleted = false);
}
