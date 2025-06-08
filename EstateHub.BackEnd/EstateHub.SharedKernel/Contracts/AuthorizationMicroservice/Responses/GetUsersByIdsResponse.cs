namespace EstateHub.SharedKernel.Contracts.AuthorizationMicroservice.Responses;

public class GetUsersByIdsResponse
{
    public List<GetUserResponse> Users { get; set; }
}
